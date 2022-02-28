using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderService.Contexts;
using Newtonsoft.Json;
using OrderService.Models;

namespace OrderService.HostedServices
{

    public class InsertOrderHostedService : BackgroundService, IDisposable
    {
        private readonly OrderModelContext _orderModelContext;
        private readonly ILogger _logger;
        private ConnectionFactory _factory;
        private IConnection _conn;
        private IModel _channel;

        private String RabbitMQ_HostName = null;
        private Int32 RabbitMQ_Port;
        private String RabbitMQ_UserName = null;
        private String RabbitMQ_Password = null;

        public InsertOrderHostedService(
            ILoggerFactory loggerFactory,
            OrderModelContext context)
        {
            _logger = loggerFactory.CreateLogger<InsertOrderHostedService>();
            _orderModelContext = context;

            initMessageServiceEnvironment();
            initMessageServiceConnection();
            initMessageServiceChannel();
        }

        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public void initMessageServiceEnvironment()
        {
            try
            {
                //generate secret in cmd:
                //powershell "[convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes(\"Hello world!\"))"

                RabbitMQ_HostName = Environment.GetEnvironmentVariable("ENV_RABBITMQ_HOST");
                RabbitMQ_Port = Convert.ToInt32(Environment.GetEnvironmentVariable("ENV_RABBITMQ_PORT"));
                RabbitMQ_UserName = Base64Decode(Environment.GetEnvironmentVariable("ENV_RABBITMQ_USERNAME"));
                RabbitMQ_Password = Base64Decode(Environment.GetEnvironmentVariable("ENV_RABBITMQ_PASSWORD"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot Initiate RabbitMQ environment");
            }
        }

        public void initMessageServiceConnection()
        {
            try
            {
                _logger.LogInformation("Initiating connection to Rabbit MQ");
                _logger.LogDebug($"Connection to Rabbit MQ using Hostname={RabbitMQ_HostName} Port={RabbitMQ_Port}");

                _factory = new ConnectionFactory() { HostName = RabbitMQ_HostName, Port = RabbitMQ_Port };
                _factory.UserName = RabbitMQ_UserName;
                _factory.Password = RabbitMQ_Password;

                _conn = _factory.CreateConnection();
                _logger.LogInformation($"Connected to Rabbit MQ using Hostname={RabbitMQ_HostName} Port={RabbitMQ_Port}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot Initiate RabbitMQ connection on Hostname={RabbitMQ_HostName} Port={RabbitMQ_Port}");
            }
        }

        public void initMessageServiceChannel()
        {
            try
            {
                _channel = _conn.CreateModel();
                //_channel.ExchangeDeclare("payment", ExchangeType.Topic);
                _channel.QueueDeclare("TaskQueue", false, false, false, null);
                //_channel.QueueBind("TaskQueue", "", "TaskQueue", null);
                // _channel.BasicQos(0, 1, false);

                _conn.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot Initiate RabbitMQ channel");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            try
            {
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (ch, ea) =>
                {
                    // received message  
                    var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

                    // handle the received message  
                    HandleMessage(content);
                    //_channel.BasicAck(ea.DeliveryTag, false);
                };

                _channel.BasicConsume(
                    queue: "TaskQueue",
                    autoAck: true,
                    consumer: consumer);

                consumer.Shutdown += OnConsumerShutdown;
                consumer.Registered += OnConsumerRegistered;
                consumer.Unregistered += OnConsumerUnregistered;
                consumer.ConsumerCancelled += OnConsumerConsumerCancelled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot consume RabbitMQ channel");
            }
            return Task.CompletedTask;
        }

        private async void HandleMessage(string content)
        {
            // we just print this message   
            _logger.LogInformation($"[x] [Consumer] received: {content}");

            try
            {
                OrderModel orderModel = JsonConvert.DeserializeObject<OrderModel>(content);

                int bookingID =
                    await _orderModelContext.InsertOrderItems(
                        orderModel);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }
        public override void Dispose()
        {
            _channel.Close();
            _conn.Close();
            base.Dispose();
        }
    }
}
