#region Copyright Preamble

// 
//    Copyright @ 2019 NCode Group
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

using GreenPipes;
using MassTransit.Extensions.Hosting;
using MassTransit.Extensions.Hosting.ActiveMq;
using MassTransit.Extensions.Hosting.AmazonSqs;
using MassTransit.Extensions.Hosting.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Example.ConsoleHost
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            // use the new generic host from ASP.NET Core
            // see for more info: https://github.com/aspnet/Hosting/issues/1163
            new HostBuilder()
                .ConfigureHostConfiguration(config => config.AddEnvironmentVariables())
                .ConfigureAppConfiguration((context, builder) => ConfigureAppConfiguration(context, builder, args))
                .ConfigureServices(ConfigureServices)
                .Build()
                .Run();
        }

        private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder configurationBuilder, string[] args)
        {
            var environmentName = context.HostingEnvironment.EnvironmentName;

            configurationBuilder.AddJsonFile("appsettings.json", optional: true);
            configurationBuilder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);

            configurationBuilder.AddCommandLine(args);
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddTransient<IWidgetService, WidgetService>();
            // add other DI services...

            // optionally use configuration for any settings
            var configuration = context.Configuration;

            // the following adds IBusManager which is also an IHostedService that is started/stopped by HostBuilder
            services.AddMassTransit(busBuilder =>
            {
                // configure RabbitMQ
                busBuilder.UseRabbitMq(configuration.GetSection("MassTransit:RabbitMq"), hostBuilder =>
                {
                    // use scopes for all downstream filters and consumers
                    // i.e. per-request lifetime
                    hostBuilder.UseServiceScope();

                    // example adding an optional configurator to the bus
                    // using IRabbitMqBusFactoryConfigurator
                    hostBuilder.AddConfigurator(configureBus =>
                    {
                        configureBus.UseRetry(r => r.Immediate(1));
                    });

                    // example adding a receive endpoint to the bus
                    hostBuilder.AddReceiveEndpoint("example-queue-1", endpointBuilder =>
                    {
                        // example adding an optional configurator to the receive endpoint
                        // using IRabbitMqReceiveEndpointConfigurator
                        endpointBuilder.AddConfigurator(configureEndpoint =>
                        {
                            configureEndpoint.UseRetry(r => r.Immediate(3));
                        });

                        // example adding a consumer to the receive endpoint
                        endpointBuilder.AddConsumer<ExampleConsumer>(configureConsumer =>
                        {
                            // example adding an optional configurator to the consumer
                            // using IConsumerConfigurator<TConsumer>
                            configureConsumer.UseRateLimit(10);
                        });
                    });
                });

                // adding more bus instances...

                // InMemory
                busBuilder.UseInMemory("connection-name-2", hostBuilder =>
                {
                    hostBuilder.UseServiceScope();
                    hostBuilder.AddReceiveEndpoint("example-queue-2", endpointBuilder =>
                    {
                        endpointBuilder.AddConsumer<ExampleConsumer>();
                    });
                });

                // ActiveMQ
                busBuilder.UseActiveMq(configuration.GetSection("MassTransit:ActiveMq"), hostBuilder =>
                {
                    hostBuilder.UseServiceScope();
                    hostBuilder.AddReceiveEndpoint("example-queue-3", endpointBuilder =>
                    {
                        endpointBuilder.AddConsumer<ExampleConsumer>();
                    });
                });

                // AmazonSQS
                busBuilder.UseAmazonSqs(configuration.GetSection("MassTransit:AmazonSqs"), hostBuilder =>
                {
                    hostBuilder.UseServiceScope();
                    hostBuilder.AddReceiveEndpoint("example-queue-4", endpointBuilder =>
                    {
                        endpointBuilder.AddConsumer<ExampleConsumer>();
                    });
                });

            });
        }

    }
}