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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.HttpTransport;
using MassTransit.HttpTransport.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace MassTransit.Extensions.Hosting.Http.Tests
{
    /// <summary />
    public class HttpHostBuilderTests
    {
        /// <summary />
        public HttpHostBuilderTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        private readonly ITestOutputHelper _output;

        /// <summary />
        [Fact]
        public async Task UseHttp_GivenStaticOptions_ThenValid()
        {
            var options = new HttpOptions
            {
                ConnectionName = "connection-name-test",
                Scheme = "http",
                Host = "127.0.0.1",
                Port = 8080,
                Method = "POST",
            };

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockHttpBusFactory = new Mock<IBusFactory<IHttpBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockHttpBusFactoryConfigurator = new Mock<IHttpBusFactoryConfigurator>(MockBehavior.Strict);
            var mockHttpHost = new Mock<IHttpHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockHttpBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IHttpBusFactoryConfigurator>>()))
                .Callback((Action<IHttpBusFactoryConfigurator> configure) => configure(mockHttpBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockHttpBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<HttpHostSettings>()))
                .Callback((HttpHostSettings settings) =>
                {
                    Assert.Equal(options.Scheme, settings.Scheme);
                    Assert.Equal(options.Host, settings.Host);
                    Assert.Equal(options.Port, settings.Port);
                    Assert.Same(HttpMethod.Post, settings.Method);
                    Assert.Equal($"{options.Host}:{options.Port}", settings.Description);
                })
                .Returns(mockHttpHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockHttpBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseHttp(options);
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(options.ConnectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockHttpHost.Verify();
            mockHttpBusFactoryConfigurator.Verify();
            mockHttpBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseHttp_GivenConfigurationOptions_ThenValid()
        {
            var options = new HttpOptions
            {
                ConnectionName = "connection-name-test",
                Scheme = "http",
                Host = "127.0.0.1",
                Port = 8080,
                Method = "FOO",
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new[]
            {
                KeyValuePairFactory.Create("MassTransit:Http:ConnectionName", options.ConnectionName),
                KeyValuePairFactory.Create("MassTransit:Http:Scheme", options.Scheme),
                KeyValuePairFactory.Create("MassTransit:Http:Host", options.Host),
                KeyValuePairFactory.Create("MassTransit:Http:Port", options.Port.ToString()),
                KeyValuePairFactory.Create("MassTransit:Http:Method", options.Method),
            });
            var configurationRoot = configurationBuilder.Build();
            var configuration = configurationRoot.GetSection("MassTransit:Http");

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockHttpBusFactory = new Mock<IBusFactory<IHttpBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockHttpBusFactoryConfigurator = new Mock<IHttpBusFactoryConfigurator>(MockBehavior.Strict);
            var mockHttpHost = new Mock<IHttpHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockHttpBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IHttpBusFactoryConfigurator>>()))
                .Callback((Action<IHttpBusFactoryConfigurator> configure) => configure(mockHttpBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockHttpBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<HttpHostSettings>()))
                .Callback((HttpHostSettings settings) =>
                {
                    Assert.Equal(options.Scheme, settings.Scheme);
                    Assert.Equal(options.Host, settings.Host);
                    Assert.Equal(options.Port, settings.Port);
                    Assert.Same(HttpMethod.Post, settings.Method);
                    Assert.Equal($"{options.Host}:{options.Port}", settings.Description);
                })
                .Returns(mockHttpHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockHttpBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseHttp(configuration, hostBuilder =>
                {
                    hostBuilder.UseMethod(HttpMethod.Post);
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

            mockHttpHost.Verify();
            mockHttpBusFactoryConfigurator.Verify();
            mockHttpBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task UseHttp_GivenUri_ThenValid()
        {
            const string connectionName = "connection-name-test";
            var hostAddress = new Uri("http://127.0.0.1:8080");

            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockHttpBusFactory = new Mock<IBusFactory<IHttpBusFactoryConfigurator>>(MockBehavior.Strict);
            var mockHttpBusFactoryConfigurator = new Mock<IHttpBusFactoryConfigurator>(MockBehavior.Strict);
            var mockHttpHost = new Mock<IHttpHost>(MockBehavior.Strict);

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockHttpBusFactory
                .Setup(_ => _.Create(It.IsAny<Action<IHttpBusFactoryConfigurator>>()))
                .Callback((Action<IHttpBusFactoryConfigurator> configure) => configure(mockHttpBusFactoryConfigurator.Object))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockHttpBusFactoryConfigurator
                .Setup(_ => _.Host(It.IsAny<HttpHostSettings>()))
                .Callback((HttpHostSettings settings) =>
                {
                    Assert.Equal(hostAddress.Scheme, settings.Scheme);
                    Assert.Equal(hostAddress.Host, settings.Host);
                    Assert.Equal(hostAddress.Port, settings.Port);
                    Assert.Same(HttpMethod.Post, settings.Method);
                    Assert.Equal($"{settings.Host}:{settings.Port}", settings.Description);
                })
                .Returns(mockHttpHost.Object)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockHttpBusFactory.Object);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseHttp(connectionName, hostAddress, HttpMethod.Post);
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                var bus = busManager.GetBus(connectionName);
                Assert.NotNull(bus);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            mockHttpHost.Verify();
            mockHttpBusFactoryConfigurator.Verify();
            mockHttpBusFactory.Verify();
            mockBusControl.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public void UseHttp_ThenHttpBusFactory()
        {
            const string connectionName = "connection-name-test";
            var hostAddress = new Uri("http://127.0.0.1:8080");

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            services.AddMassTransit(builder =>
            {
                builder.UseHttp(connectionName, hostAddress);
            });

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Transient &&
                item.ServiceType == typeof(IBusFactory<IHttpBusFactoryConfigurator>) &&
                item.ImplementationType == typeof(HttpBusFactory));
        }

    }
}