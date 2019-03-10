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
using MassTransit.ActiveMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace MassTransit.Extensions.Hosting.ActiveMq.Tests
{
    /// <summary />
    public class ActiveMqHostBuilderTests
    {
        /// <summary />
        public ActiveMqHostBuilderTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        private readonly ITestOutputHelper _output;

        /// <summary />
        [Fact]
        public async Task UseActiveMq_GivenStaticOptions_ThenValid()
        {
            var options = new ActiveMqOptions
            {
                ConnectionName = "connection-name-test",
                Host = "127.0.0.1",
                Port = 61616,
                Username = "username-test",
                Password = "password-test",
            };

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockActiveMqBusFactory = new Mock<IBusFactory<IActiveMqBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockActiveMqBusFactoryConfigurator = new Mock<IActiveMqBusFactoryConfigurator>(MockBehavior.Strict);
            var mockActiveMqHost = new Mock<IActiveMqHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockActiveMqBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IActiveMqBusFactoryConfigurator>>()))
                .Callback((Action<IActiveMqBusFactoryConfigurator> configure) => configure(mockActiveMqBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockActiveMqBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<ActiveMqHostSettings>()))
                .Callback((ActiveMqHostSettings settings) =>
                {
                    Assert.Equal(options.Host, settings.Host);
                    Assert.Equal(options.Port, settings.Port);
                    Assert.Equal(options.Username, settings.Username);
                    Assert.Equal(options.Password, settings.Password);
                })
                .Returns(mockActiveMqHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockActiveMqBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseActiveMq(options);
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(options.ConnectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockActiveMqHost.Verify();
            mockActiveMqBusFactoryConfigurator.Verify();
            mockActiveMqBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseActiveMq_GivenConfigurationOptions_ThenValid()
        {
            var options = new ActiveMqOptions
            {
                ConnectionName = "connection-name-test",
                Host = "127.0.0.1",
                Port = 61617,
                UseSsl = true,
                Username = "username-test",
                Password = "password-test",
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new[]
            {
                KeyValuePair.Create("MassTransit:ActiveMq:ConnectionName", options.ConnectionName),
                KeyValuePair.Create("MassTransit:ActiveMq:HostAddress", options.HostAddress.ToString()),
                KeyValuePair.Create("MassTransit:ActiveMq:UseSsl", options.UseSsl.ToString()),
                KeyValuePair.Create("MassTransit:ActiveMq:Username", options.Username),
                KeyValuePair.Create("MassTransit:ActiveMq:Password", options.Password),
            });
            var configurationRoot = configurationBuilder.Build();
            var configuration = configurationRoot.GetSection("MassTransit:ActiveMq");

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockActiveMqBusFactory = new Mock<IBusFactory<IActiveMqBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockActiveMqBusFactoryConfigurator = new Mock<IActiveMqBusFactoryConfigurator>(MockBehavior.Strict);
            var mockActiveMqHost = new Mock<IActiveMqHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockActiveMqBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IActiveMqBusFactoryConfigurator>>()))
                .Callback((Action<IActiveMqBusFactoryConfigurator> configure) => configure(mockActiveMqBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockActiveMqBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<ActiveMqHostSettings>()))
                .Callback((ActiveMqHostSettings settings) =>
                {
                    Assert.Equal(options.Host, settings.Host);
                    Assert.Equal(options.Port, settings.Port);
                    Assert.Equal(options.UseSsl, settings.UseSsl);
                    Assert.Equal(options.Username, settings.Username);
                    Assert.Equal(options.Password, settings.Password);
                })
                .Returns(mockActiveMqHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockActiveMqBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseActiveMq(configuration);
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(options.ConnectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockActiveMqHost.Verify();
            mockActiveMqBusFactoryConfigurator.Verify();
            mockActiveMqBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseActiveMq_GivenStringsNoPort_ThenValid()
        {
            const string connectionName = "connection-name-test";
            const string host = "127.0.0.1";
            const string username = "username-test";
            const string password = "password-test";

            var uriBuilder = new UriBuilder("activemq", host)
            {
                Port = ActiveMqOptions.DefaultPort,
            };
            var hostAddressWithPort = uriBuilder.Uri;

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockActiveMqBusFactory = new Mock<IBusFactory<IActiveMqBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockActiveMqBusFactoryConfigurator = new Mock<IActiveMqBusFactoryConfigurator>(MockBehavior.Strict);
            var mockActiveMqHost = new Mock<IActiveMqHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockActiveMqBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IActiveMqBusFactoryConfigurator>>()))
                .Callback((Action<IActiveMqBusFactoryConfigurator> configure) => configure(mockActiveMqBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockActiveMqBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<ActiveMqHostSettings>()))
                .Callback((ActiveMqHostSettings settings) =>
                {
                    Assert.Equal(host, settings.Host);
                    Assert.Equal(hostAddressWithPort, settings.HostAddress);
                    Assert.Equal(ActiveMqOptions.DefaultPort, settings.Port);
                    Assert.Equal(username, settings.Username);
                    Assert.Equal(password, settings.Password);
                })
                .Returns(mockActiveMqHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockActiveMqBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseActiveMq(connectionName, host, hostConfigurator =>
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

            mockActiveMqHost.Verify();
            mockActiveMqBusFactoryConfigurator.Verify();
            mockActiveMqBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseActiveMq_GivenUri_ThenValid()
        {
            const string connectionName = "connection-name-test";
            const string host = "127.0.0.1";
            const string username = "username-test";
            const string password = "password-test";

            var uriBuilder = new UriBuilder("activemq", host);
            var hostAddressWithoutPort = uriBuilder.Uri;

            uriBuilder.Port = ActiveMqOptions.DefaultPort;
            var hostAddressWithPort = uriBuilder.Uri;

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockActiveMqBusFactory = new Mock<IBusFactory<IActiveMqBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockActiveMqBusFactoryConfigurator = new Mock<IActiveMqBusFactoryConfigurator>(MockBehavior.Strict);
            var mockActiveMqHost = new Mock<IActiveMqHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockActiveMqBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IActiveMqBusFactoryConfigurator>>()))
                .Callback((Action<IActiveMqBusFactoryConfigurator> configure) => configure(mockActiveMqBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockActiveMqBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<ActiveMqHostSettings>()))
                .Callback((ActiveMqHostSettings settings) =>
                {
                    Assert.Equal(host, settings.Host);
                    Assert.Equal(hostAddressWithPort, settings.HostAddress);
                    Assert.Equal(username, settings.Username);
                    Assert.Equal(password, settings.Password);
                })
                .Returns(mockActiveMqHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockActiveMqBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseActiveMq(connectionName, hostAddressWithoutPort, hostConfigurator =>
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

            mockActiveMqHost.Verify();
            mockActiveMqBusFactoryConfigurator.Verify();
            mockActiveMqBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public void UseActiveMq_ThenActiveMqBusFactory()
        {
            const string connectionName = "connection-name-test";
            var hostAddress = new Uri("activemq://127.0.0.1/");

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseActiveMq(connectionName, hostAddress);
            });

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Transient &&
                item.ServiceType == typeof(IBusFactory<IActiveMqBusFactoryConfigurator>) &&
                item.ImplementationType == typeof(ActiveMqBusFactory));
        }

    }
}