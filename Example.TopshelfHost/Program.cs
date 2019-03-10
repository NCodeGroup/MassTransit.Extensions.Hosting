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

using System;
using System.Reflection;
using System.Threading;
using MassTransit.Extensions.Hosting;
using MassTransit.Extensions.Hosting.RabbitMq;
using MassTransit.ExtensionsLoggingIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Topshelf;
using Topshelf.Logging;

namespace Example.TopshelfHost
{
    public static class Program
    {
        private static readonly string EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        private static void Main()
        {
            var configuration = LoadConfiguration();
            using (var services = CreateServiceProvider(configuration))
            {
                Run(services);
            }
        }

        private static IConfiguration LoadConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();

            configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            configBuilder.AddJsonFile($"appsettings.{EnvironmentName}.json", optional: true, reloadOnChange: true);

            configBuilder.AddEnvironmentVariables();

            // WARNING: Do not add the command line arguments to the configuration
            // builder. The arguments used by Topshelf are not compatible with the
            // .NET Core command line provider.
            // configBuilder.AddCommandLine(args);

            var configuration = configBuilder.Build();
            return configuration;
        }

        private static ServiceProvider CreateServiceProvider(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            ConfigureServices(configuration, services);

            return services.BuildServiceProvider();
        }

        private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton(configuration);

            // configure logging
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));

                builder.AddNLog();
                builder.AddDebug();
            });

            // the following adds IBusManager which is also an IHostedService that is started/stopped down below
            services.AddMassTransit(busBuilder =>
            {
                // configure RabbitMQ
                busBuilder.UseRabbitMq(configuration.GetSection("MassTransit:RabbitMq"), hostBuilder =>
                {
                    hostBuilder.UseServiceScope();

                    hostBuilder.AddReceiveEndpoint("example-queue-1", endpointBuilder =>
                    {
                        endpointBuilder.AddConsumer<ExampleConsumer>();
                    });
                });

                // see `Example.ConsoleHost` for examples with other transports such as ActiveMq, etc.
            });
        }

        private static void Run(IServiceProvider serviceProvider)
        {
            ExtensionsLogger.Use(); // MassTransit
            NLogLogWriterFactory.Use(); // Topshelf

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(Program));

            logger.LogInformation("Program Starting");
            logger.LogInformation("Environment={0}", EnvironmentName);
            logger.LogInformation(Assembly.GetExecutingAssembly().FullName);

            // configure and run the topshelf service
            HostFactory.Run(hostConfigurator =>
            {
                // log exception details
                hostConfigurator.OnException(exception => logger.LogError(exception.ToString()));

                // configure the hosted service
                hostConfigurator.Service<IHostedService>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(serviceProvider.GetRequiredService<IHostedService>);
                    serviceConfigurator.WhenStarted(async host => await host.StartAsync(CancellationToken.None).ConfigureAwait(false));
                    serviceConfigurator.WhenStopped(async host => await host.StopAsync(CancellationToken.None).ConfigureAwait(false));
                });

                // by default use a least privileged account
                hostConfigurator.RunAsNetworkService();

                // default the ServiceName, DisplayName, and Description from the entry assembly
                hostConfigurator.UseAssemblyInfoForServiceInfo();
            });

            logger.LogInformation("Program Exiting");
        }

    }
}