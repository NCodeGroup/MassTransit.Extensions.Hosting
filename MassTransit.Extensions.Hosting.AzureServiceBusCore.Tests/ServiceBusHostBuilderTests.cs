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
using System.Threading;
using System.Threading.Tasks;
using MassTransit.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace MassTransit.Extensions.Hosting.AzureServiceBusCore.Tests
{
    /// <summary />
    public class ServiceBusHostBuilderTests
    {
        /// <summary />
        public ServiceBusHostBuilderTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        private readonly ITestOutputHelper _output;

        /// <summary />
        [Fact]
        public async Task UseAzureServiceBus_GivenStaticOptions_ThenValid()
        {
            var options = new ServiceBusOptions
            {
                ConnectionName = "connection-name-test",
                HostAddress = new Uri("sb://namespace.servicebus.windows.net/scope"),
                TokenProvider = TokenProvider.CreateManagedServiceIdentityTokenProvider(),
                OperationTimeout = TimeSpan.FromMilliseconds(101),
                RetryMinBackoff = TimeSpan.FromMilliseconds(102),
                RetryMaxBackoff = TimeSpan.FromMilliseconds(103),
                RetryLimit = 9,
                TransportType = TransportType.AmqpWebSockets,
            };

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockAzureServiceBusBusFactory = new Mock<IBusFactory<IServiceBusBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockAzureServiceBusBusFactoryConfigurator = new Mock<IServiceBusBusFactoryConfigurator>(MockBehavior.Strict);
            var mockAzureServiceBusHost = new Mock<IServiceBusHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockAzureServiceBusBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IServiceBusBusFactoryConfigurator>>()))
                .Callback((Action<IServiceBusBusFactoryConfigurator> configure) => configure(mockAzureServiceBusBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockAzureServiceBusBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<ServiceBusHostSettings>()))
                .Callback((ServiceBusHostSettings settings) =>
                {
                    Assert.Equal(options.HostAddress, settings.ServiceUri);
                    Assert.Same(options.TokenProvider, settings.TokenProvider);
                    Assert.Equal(options.OperationTimeout, settings.OperationTimeout);
                    Assert.Equal(options.RetryMinBackoff, settings.RetryMinBackoff);
                    Assert.Equal(options.RetryMaxBackoff, settings.RetryMaxBackoff);
                    Assert.Equal(options.RetryLimit, settings.RetryLimit);
                    Assert.Equal(options.TransportType, settings.TransportType);
                })
                .Returns(mockAzureServiceBusHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockAzureServiceBusBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseAzureServiceBus(options);
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(options.ConnectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockAzureServiceBusHost.Verify();
            mockAzureServiceBusBusFactoryConfigurator.Verify();
            mockAzureServiceBusBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseAzureServiceBus_GivenConfigurationOptions_ThenValid()
        {
            var options = new ServiceBusOptions
            {
                ConnectionName = "connection-name-test",
                HostAddress = new Uri("sb://namespace.servicebus.windows.net/scope"),
                TokenProvider = TokenProvider.CreateManagedServiceIdentityTokenProvider(),
                OperationTimeout = TimeSpan.FromMilliseconds(101),
                RetryMinBackoff = TimeSpan.FromMilliseconds(102),
                RetryMaxBackoff = TimeSpan.FromMilliseconds(103),
                RetryLimit = 9,
                TransportType = TransportType.AmqpWebSockets,
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new[]
            {
                KeyValuePairFactory.Create("MassTransit:AzureServiceBus:ConnectionName", options.ConnectionName),
                KeyValuePairFactory.Create("MassTransit:AzureServiceBus:HostAddress", options.HostAddress.ToString()),
                KeyValuePairFactory.Create("MassTransit:AzureServiceBus:OperationTimeout", options.OperationTimeout.ToString()),
                KeyValuePairFactory.Create("MassTransit:AzureServiceBus:RetryMinBackoff", options.RetryMinBackoff.ToString()),
                KeyValuePairFactory.Create("MassTransit:AzureServiceBus:RetryMaxBackoff", options.RetryMaxBackoff.ToString()),
                KeyValuePairFactory.Create("MassTransit:AzureServiceBus:RetryLimit", options.RetryLimit.ToString()),
                KeyValuePairFactory.Create("MassTransit:AzureServiceBus:TransportType", options.TransportType.ToString()),
            });
            var configurationRoot = configurationBuilder.Build();
            var configuration = configurationRoot.GetSection("MassTransit:AzureServiceBus");

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockAzureServiceBusBusFactory = new Mock<IBusFactory<IServiceBusBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockAzureServiceBusBusFactoryConfigurator = new Mock<IServiceBusBusFactoryConfigurator>(MockBehavior.Strict);
            var mockAzureServiceBusHost = new Mock<IServiceBusHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockAzureServiceBusBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IServiceBusBusFactoryConfigurator>>()))
                .Callback((Action<IServiceBusBusFactoryConfigurator> configure) => configure(mockAzureServiceBusBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockAzureServiceBusBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<ServiceBusHostSettings>()))
                .Callback((ServiceBusHostSettings settings) =>
                {
                    Assert.Equal(options.HostAddress, settings.ServiceUri);
                    Assert.Same(options.TokenProvider, settings.TokenProvider);
                    Assert.Equal(options.OperationTimeout, settings.OperationTimeout);
                    Assert.Equal(options.RetryMinBackoff, settings.RetryMinBackoff);
                    Assert.Equal(options.RetryMaxBackoff, settings.RetryMaxBackoff);
                    Assert.Equal(options.RetryLimit, settings.RetryLimit);
                    Assert.Equal(options.TransportType, settings.TransportType);
                })
                .Returns(mockAzureServiceBusHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockAzureServiceBusBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseAzureServiceBus(configuration, hostBuilder =>
                {
                    hostBuilder.UseTokenProvider(options.TokenProvider);
                });
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(options.ConnectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockAzureServiceBusHost.Verify();
            mockAzureServiceBusBusFactoryConfigurator.Verify();
            mockAzureServiceBusBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseAzureServiceBus_GivenUri_ThenValid()
        {
            const string connectionName = "connection-name-test";
            var hostAddress = new Uri("sb://namespace.servicebus.windows.net/scope");

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockAzureServiceBusBusFactory = new Mock<IBusFactory<IServiceBusBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockAzureServiceBusBusFactoryConfigurator = new Mock<IServiceBusBusFactoryConfigurator>(MockBehavior.Strict);
            var mockAzureServiceBusHost = new Mock<IServiceBusHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockAzureServiceBusBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IServiceBusBusFactoryConfigurator>>()))
                .Callback((Action<IServiceBusBusFactoryConfigurator> configure) => configure(mockAzureServiceBusBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockAzureServiceBusBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<ServiceBusHostSettings>()))
                .Callback((ServiceBusHostSettings settings) =>
                {
                    Assert.Equal(hostAddress, settings.ServiceUri);
                })
                .Returns(mockAzureServiceBusHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockAzureServiceBusBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseAzureServiceBus(connectionName, hostAddress);
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(connectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockAzureServiceBusHost.Verify();
            mockAzureServiceBusBusFactoryConfigurator.Verify();
            mockAzureServiceBusBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public void UseAzureServiceBus_ThenAzureServiceBusBusFactory()
        {
            const string connectionName = "connection-name-test";
            var hostAddress = new Uri("sb://namespace.servicebus.windows.net/scope");

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseAzureServiceBus(connectionName, hostAddress);
            });

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Transient &&
                item.ServiceType == typeof(IBusFactory<IServiceBusBusFactoryConfigurator>) &&
                item.ImplementationType == typeof(ServiceBusBusFactory));
        }

    }
}