using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using System.Text;

namespace OrderService.Services
{
    public interface IMessageService
    {
        void initMessageServiceEnvironment();
        void initMessageServiceConnection();
        bool enqueue(string message,
            string queue,
            string exchange,
            string routingKey);
        void Dispose();
    }

    public class MessageService : IDisposable, IMessageService
    {
        private readonly ILogger _logger;

        private ConnectionFactory _factory;
        private IConnection _conn;
        private IModel _channel;

        private String RabbitMQ_HostName = null;
        private Int32 RabbitMQ_Port;
        private String RabbitMQ_UserName = null;
        private String RabbitMQ_Password = null;

        public MessageService(
            ILogger<MessageService> logger)
        {
            _logger = logger;

            initMessageServiceEnvironment();
            initMessageServiceConnection();
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

        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public void initMessageServiceConnection()
        {
            try
            {
                _logger.LogInformation("Initiating connection to Rabbit MQ");

                _logger.LogInformation($"Connecting to Rabbit MQ using Hostname={RabbitMQ_HostName} Port={RabbitMQ_Port}");
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
        public bool enqueue(
            string messageString,
            string queue,
            string exchange,
            string routingKey)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes("server processed " + messageString);

                _channel = _conn.CreateModel();
                _channel.ExchangeDeclare(exchange, ExchangeType.Topic);
                _channel.QueueDeclare(queue: queue,
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

                _channel.BasicPublish(exchange: exchange,
                                    routingKey: routingKey,
                                    basicProperties: null,
                                    body: body);
                Console.WriteLine("[x] Published {0} to RabbitMQ", messageString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot produce message to RabbitMQ");
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            try
            {
                _channel.Close();
                _channel.Dispose();
                _channel = null;

                _conn.Close();
                _conn.Dispose();
                _conn = null;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Cannot dispose RabbitMQ channel or connection");
            }
        }
    }
}
