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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace MassTransit.Extensions.Hosting.RabbitMq.Tests
{
    /// <summary />
    public class RabbitMqHostBuilderTests
    {
        /// <summary />
        public RabbitMqHostBuilderTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        private readonly ITestOutputHelper _output;

        /// <summary />
        [Fact]
        public async Task UseRabbitMq_GivenStaticOptions_ThenValid()
        {
            var options = new RabbitMqOptions
            {
                ConnectionName = "connection-name-test",
                Host = "127.0.0.1",
                Port = 5672,
                VirtualHost = "/",
                Username = "username-test",
                Password = "password-test",
            };

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockRabbitMqBusFactory = new Mock<IBusFactory<IRabbitMqBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockRabbitMqBusFactoryConfigurator = new Mock<IRabbitMqBusFactoryConfigurator>(MockBehavior.Strict);
            var mockRabbitMqHost = new Mock<IRabbitMqHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockRabbitMqBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IRabbitMqBusFactoryConfigurator>>()))
                .Callback((Action<IRabbitMqBusFactoryConfigurator> configure) => configure(mockRabbitMqBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockRabbitMqBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<RabbitMqHostSettings>()))
                .Callback((RabbitMqHostSettings settings) =>
                {
                    Assert.Equal(options.ConnectionName, settings.ClientProvidedName);
                    Assert.Equal(options.Host, settings.Host);
                    Assert.Equal(options.Port, settings.Port);
                    Assert.Equal(options.Heartbeat.GetValueOrDefault(), settings.Heartbeat);
                    Assert.Equal(options.VirtualHost, settings.VirtualHost);
                    Assert.Equal(options.Username, settings.Username);
                    Assert.Equal(options.Password, settings.Password);
                })
                .Returns(mockRabbitMqHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockRabbitMqBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseRabbitMq(options);
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(options.ConnectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockRabbitMqHost.Verify();
            mockRabbitMqBusFactoryConfigurator.Verify();
            mockRabbitMqBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseRabbitMq_GivenConfigurationOptions_ThenValid()
        {
            var options = new RabbitMqOptions
            {
                ConnectionName = "connection-name-test",
                Host = "127.0.0.1",
                Port = 5672,
                VirtualHost = "/",
                Username = "username-test",
                Password = "password-test",
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new[]
            {
                KeyValuePair.Create("MassTransit:RabbitMq:ConnectionName", options.ConnectionName),
                KeyValuePair.Create("MassTransit:RabbitMq:HostAddress", options.HostAddress.ToString()),
                KeyValuePair.Create("MassTransit:RabbitMq:Username", options.Username),
                KeyValuePair.Create("MassTransit:RabbitMq:Password", options.Password),
            });
            var configurationRoot = configurationBuilder.Build();
            var configuration = configurationRoot.GetSection("MassTransit:RabbitMq");

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockRabbitMqBusFactory = new Mock<IBusFactory<IRabbitMqBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockRabbitMqBusFactoryConfigurator = new Mock<IRabbitMqBusFactoryConfigurator>(MockBehavior.Strict);
            var mockRabbitMqHost = new Mock<IRabbitMqHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockRabbitMqBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IRabbitMqBusFactoryConfigurator>>()))
                .Callback((Action<IRabbitMqBusFactoryConfigurator> configure) => configure(mockRabbitMqBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockRabbitMqBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<RabbitMqHostSettings>()))
                .Callback((RabbitMqHostSettings settings) =>
                {
                    Assert.Equal(options.ConnectionName, settings.ClientProvidedName);
                    Assert.Equal(options.Host, settings.Host);
                    Assert.Equal(options.Port, settings.Port);
                    Assert.Equal(options.Heartbeat.GetValueOrDefault(), settings.Heartbeat);
                    Assert.Equal(options.VirtualHost, settings.VirtualHost);
                    Assert.Equal(options.Username, settings.Username);
                    Assert.Equal(options.Password, settings.Password);
                })
                .Returns(mockRabbitMqHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockRabbitMqBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseRabbitMq(configuration);
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(options.ConnectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockRabbitMqHost.Verify();
            mockRabbitMqBusFactoryConfigurator.Verify();
            mockRabbitMqBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseRabbitMq_GivenStringsNoPort_ThenValid()
        {
            const string connectionName = "connection-name-test";
            const string host = "127.0.0.1";
            const string virtualHost = "/";
            const string username = "username-test";
            const string password = "password-test";

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockRabbitMqBusFactory = new Mock<IBusFactory<IRabbitMqBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockRabbitMqBusFactoryConfigurator = new Mock<IRabbitMqBusFactoryConfigurator>(MockBehavior.Strict);
            var mockRabbitMqHost = new Mock<IRabbitMqHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockRabbitMqBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IRabbitMqBusFactoryConfigurator>>()))
                .Callback((Action<IRabbitMqBusFactoryConfigurator> configure) => configure(mockRabbitMqBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockRabbitMqBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<RabbitMqHostSettings>()))
                .Callback((RabbitMqHostSettings settings) =>
                {
                    Assert.Equal(connectionName, settings.ClientProvidedName);
                    Assert.Equal(host, settings.Host);
                    Assert.Equal(virtualHost, settings.VirtualHost);
                    Assert.Equal(username, settings.Username);
                    Assert.Equal(password, settings.Password);
                })
                .Returns(mockRabbitMqHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockRabbitMqBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseRabbitMq(connectionName, host, virtualHost, hostConfigurator =>
                {
                    hostConfigurator.UseCredentials(username, password);
                });
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(connectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockRabbitMqHost.Verify();
            mockRabbitMqBusFactoryConfigurator.Verify();
            mockRabbitMqBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseRabbitMq_GivenUri_ThenValid()
        {
            const string connectionName = "connection-name-test";
            var hostAddress = new Uri("rabbitmq://127.0.0.1/");
            const string username = "username-test";
            const string password = "password-test";

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockRabbitMqBusFactory = new Mock<IBusFactory<IRabbitMqBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockRabbitMqBusFactoryConfigurator = new Mock<IRabbitMqBusFactoryConfigurator>(MockBehavior.Strict);
            var mockRabbitMqHost = new Mock<IRabbitMqHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockRabbitMqBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IRabbitMqBusFactoryConfigurator>>()))
                .Callback((Action<IRabbitMqBusFactoryConfigurator> configure) => configure(mockRabbitMqBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockRabbitMqBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<RabbitMqHostSettings>()))
                .Callback((RabbitMqHostSettings settings) =>
                {
                    Assert.Equal(connectionName, settings.ClientProvidedName);
                    Assert.Equal(hostAddress, settings.HostAddress);
                    Assert.Equal(username, settings.Username);
                    Assert.Equal(password, settings.Password);
                })
                .Returns(mockRabbitMqHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockRabbitMqBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseRabbitMq(connectionName, hostAddress, hostConfigurator =>
                {
                    hostConfigurator.UseCredentials(username, password);
                });
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(connectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockRabbitMqHost.Verify();
            mockRabbitMqBusFactoryConfigurator.Verify();
            mockRabbitMqBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public void UseRabbitMq_ThenRabbitMqBusFactory()
        {
            const string connectionName = "connection-name-test";
            var hostAddress = new Uri("rabbitmq://127.0.0.1/");

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseRabbitMq(connectionName, hostAddress);
            });

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Transient &&
                item.ServiceType == typeof(IBusFactory<IRabbitMqBusFactoryConfigurator>) &&
                item.ImplementationType == typeof(RabbitMqBusFactory));
        }

    }
}