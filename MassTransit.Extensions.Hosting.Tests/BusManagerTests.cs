#region Copyright Preamble
// 
//    Copyright @ 2018 NCode Group
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace MassTransit.Extensions.Hosting.Tests
{
    /// <summary />
    public class BusManagerTests
    {
        private readonly ITestOutputHelper _output;

        /// <summary />
        public BusManagerTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        private ServiceProvider CreateServiceProvider(Action<IServiceCollection> configurator = null)
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            configurator?.Invoke(services);

            services.TryAddTransient<IBusManager, BusManager>();

            return services.BuildServiceProvider();
        }

        /// <summary />
        [Fact]
        public async Task Start_GivenNoFactory_ThenStarted()
        {
            using (var serviceProvider = CreateServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);
            }
        }

        /// <summary />
        [Fact]
        public async Task Start_GivenNoFactory_WhenStopped_ThenThrows()
        {
            using (var serviceProvider = CreateServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.True(busManager.Stopping.IsCancellationRequested);
                Assert.True(busManager.Stopped.IsCancellationRequested);

                await Assert.ThrowsAsync<InvalidOperationException>(() => busManager.StartAsync(CancellationToken.None)).ConfigureAwait(false);
            }
        }

        /// <summary />
        [Fact]
        public async Task Stop_GivenNoFactory_WhenNotStarted_ThenThrows()
        {
            using (var serviceProvider = CreateServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await Assert.ThrowsAsync<InvalidOperationException>(() => busManager.StopAsync(CancellationToken.None)).ConfigureAwait(false);
            }
        }

        /// <summary />
        [Fact]
        public async Task Stop_GivenNoFactory_WhenStarted_ThenStopped()
        {
            using (var serviceProvider = CreateServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.True(busManager.Stopping.IsCancellationRequested);
                Assert.True(busManager.Stopped.IsCancellationRequested);
            }
        }

        /// <summary />
        [Fact]
        public void GetBus_GivenNoFactory_WhenNotStarted_ThenThrows()
        {
            const string connectionName = "test-connection";

            using (var serviceProvider = CreateServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.Throws<InvalidOperationException>(() => busManager.GetBus(connectionName));
            }
        }

        /// <summary />
        [Fact]
        public async Task GetBus_GivenNoFactory_WhenStopped_ThenThrows()
        {
            const string connectionName = "test-connection";

            using (var serviceProvider = CreateServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.True(busManager.Stopping.IsCancellationRequested);
                Assert.True(busManager.Stopped.IsCancellationRequested);

                Assert.Throws<InvalidOperationException>(() => busManager.GetBus(connectionName));
            }
        }

        /// <summary />
        [Fact]
        public async Task Start_GivenFactory_ThenStarted()
        {
            const string connectionName = "test-connection";

            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockBusHostFactory = new Mock<IBusHostFactory>(MockBehavior.Strict);
            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);

            mockBusHostFactory
                .SetupGet(_ => _.ConnectionName)
                .Returns(connectionName)
                .Verifiable();

            mockBusHostFactory
                .Setup(_ => _.Create(It.IsAny<IServiceProvider>()))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(mockBusHostFactory.Object);
            }

            using (var serviceProvider = CreateServiceProvider(ConfigureServices))
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);
            }

            mockBusControl.Verify();
            mockBusHostFactory.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task Start_GivenFactoryException_ThenThrows()
        {
            const string connectionName = "test-connection";

            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockBusHostFactory = new Mock<IBusHostFactory>(MockBehavior.Strict);
            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);

            mockBusHostFactory
                .SetupGet(_ => _.ConnectionName)
                .Returns(connectionName)
                .Verifiable();

            mockBusHostFactory
                .Setup(_ => _.Create(It.IsAny<IServiceProvider>()))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>()
                .Verifiable();

            void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(mockBusHostFactory.Object);
            }

            using (var serviceProvider = CreateServiceProvider(ConfigureServices))
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await Assert.ThrowsAsync<InvalidOperationException>(() => busManager.StartAsync(CancellationToken.None)).ConfigureAwait(false);

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);
            }

            mockBusControl.Verify();
            mockBusHostFactory.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task Stop_GivenFactory_WhenNotStarted_ThenThrows()
        {
            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockBusHostFactory = new Mock<IBusHostFactory>(MockBehavior.Strict);

            void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(mockBusHostFactory.Object);
            }

            using (var serviceProvider = CreateServiceProvider(ConfigureServices))
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await Assert.ThrowsAsync<InvalidOperationException>(() => busManager.StopAsync(CancellationToken.None)).ConfigureAwait(false);
            }

            mockBusControl.Verify();
            mockBusHostFactory.Verify();
        }

        /// <summary />
        [Fact]
        public async Task Stop_GivenFactory_WhenStarted_ThenStopped()
        {
            const string connectionName = "test-connection";

            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockBusHostFactory = new Mock<IBusHostFactory>(MockBehavior.Strict);
            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);

            mockBusHostFactory
                .SetupGet(_ => _.ConnectionName)
                .Returns(connectionName)
                .Verifiable();

            mockBusHostFactory
                .Setup(_ => _.Create(It.IsAny<IServiceProvider>()))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(mockBusHostFactory.Object);
            }

            using (var serviceProvider = CreateServiceProvider(ConfigureServices))
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.True(busManager.Stopping.IsCancellationRequested);
                Assert.True(busManager.Stopped.IsCancellationRequested);
            }

            mockBusControl.Verify();
            mockBusHostFactory.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public void GetBus_GivenFactory_WhenNotStarted_ThenThrows()
        {
            const string connectionName = "test-connection";

            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockBusHostFactory = new Mock<IBusHostFactory>(MockBehavior.Strict);

            void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(mockBusHostFactory.Object);
            }

            using (var serviceProvider = CreateServiceProvider(ConfigureServices))
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);

                Assert.Throws<InvalidOperationException>(() => busManager.GetBus(connectionName));
            }

            mockBusControl.Verify();
            mockBusHostFactory.Verify();
        }

        /// <summary />
        [Fact]
        public async Task GetBus_GivenFactory_WhenStopped_ThenThrows()
        {
            const string connectionName = "test-connection";

            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockBusHostFactory = new Mock<IBusHostFactory>(MockBehavior.Strict);
            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);

            mockBusHostFactory
                .SetupGet(_ => _.ConnectionName)
                .Returns(connectionName)
                .Verifiable();

            mockBusHostFactory
                .Setup(_ => _.Create(It.IsAny<IServiceProvider>()))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(mockBusHostFactory.Object);
            }

            using (var serviceProvider = CreateServiceProvider(ConfigureServices))
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.False(busManager.Stopping.IsCancellationRequested);
                Assert.False(busManager.Stopped.IsCancellationRequested);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);
                Assert.True(busManager.Stopping.IsCancellationRequested);
                Assert.True(busManager.Stopped.IsCancellationRequested);

                Assert.Throws<InvalidOperationException>(() => busManager.GetBus(connectionName));
            }

            mockBusControl.Verify();
            mockBusHostFactory.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task GetBus_GivenFactoryWrongName_WhenStarted_ThenThrows()
        {
            const string registeredConnection = "test-connection";
            const string otherConnection = "other-connection";

            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockBusHostFactory = new Mock<IBusHostFactory>(MockBehavior.Strict);
            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);

            mockBusHostFactory
                .SetupGet(_ => _.ConnectionName)
                .Returns(registeredConnection)
                .Verifiable();

            mockBusHostFactory
                .Setup(_ => _.Create(It.IsAny<IServiceProvider>()))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(mockBusHostFactory.Object);
            }

            using (var serviceProvider = CreateServiceProvider(ConfigureServices))
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);

                Assert.Throws<KeyNotFoundException>(() => busManager.GetBus(otherConnection));
            }

            mockBusControl.Verify();
            mockBusHostFactory.Verify();
            mockBusHandle.Verify();
        }

        /// <summary />
        [Fact]
        public async Task GetBus_GivenFactory_WhenStarted_ThenSuccess()
        {
            const string connectionName = "test-connection";

            var mockBusControl = new Mock<IBusControl>(MockBehavior.Strict);
            var mockBusHostFactory = new Mock<IBusHostFactory>(MockBehavior.Strict);
            var mockBusHandle = new Mock<BusHandle>(MockBehavior.Strict);

            mockBusHostFactory
                .SetupGet(_ => _.ConnectionName)
                .Returns(connectionName)
                .Verifiable();

            mockBusHostFactory
                .Setup(_ => _.Create(It.IsAny<IServiceProvider>()))
                .Returns(mockBusControl.Object)
                .Verifiable();

            mockBusControl
                .Setup(_ => _.StartAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBusHandle.Object)
                .Verifiable();

            void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(mockBusHostFactory.Object);
            }

            using (var serviceProvider = CreateServiceProvider(ConfigureServices))
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                Assert.False(busManager.Started.IsCancellationRequested);

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.True(busManager.Started.IsCancellationRequested);

                var bus = busManager.GetBus(connectionName);
                Assert.NotNull(bus);
            }

            mockBusControl.Verify();
            mockBusHostFactory.Verify();
            mockBusHandle.Verify();
        }

    }
}