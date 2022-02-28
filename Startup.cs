using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderService.HostedServices;
using OrderService.Services;

namespace OrderService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddHostedService<InsertOrderHostedService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderService", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostApplicationLifetime lifetime,
            IWebHostEnvironment env,
            IMessageService messageservice)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Check for lifetime starting up working with messaging service
            lifetime.ApplicationStarted.Register(() => {
                Console.WriteLine("*** Application is starting up...");

                Console.WriteLine("*** Init message service environment...");
                messageservice.initMessageServiceEnvironment();

                Console.WriteLine("*** Init message service connection...");
                messageservice.initMessageServiceConnection();
            }, true);

            // Check for lifetime shutdown working with messaging service active
            lifetime.ApplicationStopping.Register(() => {
                Console.WriteLine("*** Application is shutting down...");

                messageservice.Dispose();
                Console.WriteLine("*** Message service disposed...");
            }, true);

            lifetime.ApplicationStopped.Register(() => {
                Console.WriteLine("*** Application is shut down...");

                messageservice.Dispose();
                Console.WriteLine("Message service disposed...");
            }, true);
        }
    }
}
