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
using Amazon;
using MassTransit.AmazonSqsTransport;
using MassTransit.AmazonSqsTransport.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace MassTransit.Extensions.Hosting.AmazonSqs.Tests
{
    /// <summary />
    public class AmazonSqsHostBuilderTests
    {
        /// <summary />
        public AmazonSqsHostBuilderTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        private readonly ITestOutputHelper _output;

        /// <summary />
        [Fact]
        public async Task UseAmazonSqs_GivenStaticOptions_ThenValid()
        {
            var options = new AmazonSqsOptions
            {
                ConnectionName = "connection-name-test",
                RegionSystemName = RegionEndpoint.USWest2.SystemName,
                AccessKey = "access-key-test",
                SecretKey = "secret-key-test",
            };

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockAmazonSqsBusFactory = new Mock<IBusFactory<IAmazonSqsBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockAmazonSqsBusFactoryConfigurator = new Mock<IAmazonSqsBusFactoryConfigurator>(MockBehavior.Strict);
            var mockAmazonSqsHost = new Mock<IAmazonSqsHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockAmazonSqsBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IAmazonSqsBusFactoryConfigurator>>()))
                .Callback((Action<IAmazonSqsBusFactoryConfigurator> configure) => configure(mockAmazonSqsBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockAmazonSqsBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<AmazonSqsHostSettings>()))
                .Callback((AmazonSqsHostSettings settings) =>
                {
                    Assert.Equal(options.RegionSystemName, settings.Region.SystemName);
                    Assert.Equal(options.AccessKey, settings.AccessKey);
                    Assert.Equal(options.SecretKey, settings.SecretKey);
                })
                .Returns(mockAmazonSqsHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockAmazonSqsBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseAmazonSqs(options);
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(options.ConnectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockAmazonSqsHost.Verify();
            mockAmazonSqsBusFactoryConfigurator.Verify();
            mockAmazonSqsBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseAmazonSqs_GivenConfigurationOptions_ThenValid()
        {
            var options = new AmazonSqsOptions
            {
                ConnectionName = "connection-name-test",
                RegionSystemName = RegionEndpoint.USWest2.SystemName,
                AccessKey = "access-key-test",
                SecretKey = "secret-key-test",

            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new[]
            {
                KeyValuePair.Create("MassTransit:AmazonSqs:ConnectionName", options.ConnectionName),
                KeyValuePair.Create("MassTransit:AmazonSqs:RegionSystemName", options.RegionSystemName),
                KeyValuePair.Create("MassTransit:AmazonSqs:AccessKey", options.AccessKey),
                KeyValuePair.Create("MassTransit:AmazonSqs:SecretKey", options.SecretKey),
            });
            var configurationRoot = configurationBuilder.Build();
            var configuration = configurationRoot.GetSection("MassTransit:AmazonSqs");

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockAmazonSqsBusFactory = new Mock<IBusFactory<IAmazonSqsBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockAmazonSqsBusFactoryConfigurator = new Mock<IAmazonSqsBusFactoryConfigurator>(MockBehavior.Strict);
            var mockAmazonSqsHost = new Mock<IAmazonSqsHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockAmazonSqsBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IAmazonSqsBusFactoryConfigurator>>()))
                .Callback((Action<IAmazonSqsBusFactoryConfigurator> configure) => configure(mockAmazonSqsBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockAmazonSqsBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<AmazonSqsHostSettings>()))
                .Callback((AmazonSqsHostSettings settings) =>
                {
                    Assert.Same(options.RegionSystemName, settings.Region.SystemName);
                    Assert.Equal(options.AccessKey, settings.AccessKey);
                    Assert.Equal(options.SecretKey, settings.SecretKey);
                })
                .Returns(mockAmazonSqsHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockAmazonSqsBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseAmazonSqs(configuration);
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(options.ConnectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockAmazonSqsHost.Verify();
            mockAmazonSqsBusFactoryConfigurator.Verify();
            mockAmazonSqsBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseAmazonSqs_GivenUri_ThenValid()
        {
            const string connectionName = "connection-name-test";
            const string accessKey = "access-key-test";
            const string secretKey = "secret-key-test";

            var region = RegionEndpoint.USWest2;
            var host = region.SystemName;
            var uriBuilder = new UriBuilder("amazonsqs", host)
            {
                UserName = accessKey,
                Password = secretKey,
            };
            var hostAddress = uriBuilder.Uri;

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockAmazonSqsBusFactory = new Mock<IBusFactory<IAmazonSqsBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockAmazonSqsBusFactoryConfigurator = new Mock<IAmazonSqsBusFactoryConfigurator>(MockBehavior.Strict);
            var mockAmazonSqsHost = new Mock<IAmazonSqsHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockAmazonSqsBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IAmazonSqsBusFactoryConfigurator>>()))
                .Callback((Action<IAmazonSqsBusFactoryConfigurator> configure) => configure(mockAmazonSqsBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockAmazonSqsBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<AmazonSqsHostSettings>()))
                .Callback((AmazonSqsHostSettings settings) =>
                {
                    Assert.Equal(region, settings.Region);
                    Assert.Equal(hostAddress, settings.HostAddress);
                    Assert.Equal(accessKey, settings.AccessKey);
                    Assert.Equal(secretKey, settings.SecretKey);
                })
                .Returns(mockAmazonSqsHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockAmazonSqsBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseAmazonSqs(connectionName, hostAddress);
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(connectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockAmazonSqsHost.Verify();
            mockAmazonSqsBusFactoryConfigurator.Verify();
            mockAmazonSqsBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public void UseAmazonSqs_ThenAmazonSqsBusFactory()
        {
            const string connectionName = "connection-name-test";
            var hostAddress = new Uri("amazonsqs://us-east-1/");

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseAmazonSqs(connectionName, hostAddress);
            });

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Transient &&
                item.ServiceType == typeof(IBusFactory<IAmazonSqsBusFactoryConfigurator>) &&
                item.ImplementationType == typeof(AmazonSqsBusFactory));
        }

    }
}