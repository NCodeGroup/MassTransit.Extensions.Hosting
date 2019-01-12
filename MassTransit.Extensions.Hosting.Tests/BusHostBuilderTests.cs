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
using GreenPipes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MassTransit.Extensions.Hosting.Tests
{
    /// <summary />
    public class BusHostBuilderTests
    {
        private static void AssertSpecifications(DummyBusHostBuilder<IHost, IBusFactoryConfigurator> busHostBuilder)
        {
            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockBusFactoryConfigurator = new Mock<IBusFactoryConfigurator>(MockBehavior.Strict);

            mockBusFactoryConfigurator
                .As<IPipeConfigurator<ConsumeContext>>()
                .Setup(_ => _.AddPipeSpecification(It.IsAny<IPipeSpecification<ConsumeContext>>()))
                .Callback((IPipeSpecification<ConsumeContext> specification) => AssertSpecification(specification))
                .Verifiable();

            busHostBuilder.DoConfigure(mockHost.Object, mockBusFactoryConfigurator.Object, mockServiceProvider.Object);

            mockBusFactoryConfigurator.Verify();
        }

        private static void AssertSpecification(ISpecification specification)
        {
            Assert.NotNull(specification);
            var validationResults = specification.Validate();
            Assert.NotNull(validationResults);
            Assert.Empty(validationResults);
        }

        /// <summary />
        [Fact]
        public void AddConfigurator_ThenValid()
        {
            var services = new ServiceCollection();
            var busHostBuilder = new DummyBusHostBuilder<IHost, IBusFactoryConfigurator>(services, "test");

            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockBusFactoryConfigurator = new Mock<IBusFactoryConfigurator>(MockBehavior.Strict);

            var configuratorWasCalled = false;
            busHostBuilder.AddConfigurator((host, configurator, serviceProvider) =>
            {
                configuratorWasCalled = true;
                Assert.Same(mockHost.Object, host);
                Assert.Same(mockServiceProvider.Object, serviceProvider);
                Assert.Same(mockBusFactoryConfigurator.Object, configurator);
            });

            busHostBuilder.DoConfigure(mockHost.Object, mockBusFactoryConfigurator.Object, mockServiceProvider.Object);
            Assert.True(configuratorWasCalled);
        }

        /// <summary />
        [Fact]
        public void AddReceiveEndpoint_GivenConfigurator_ThenValid()
        {
            var services = new ServiceCollection();
            var busHostBuilder =
                new DummyBusHostBuilder<IHost, IBusFactoryConfigurator>(services, "connection-name-test");

            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockBusFactoryConfigurator = new Mock<IBusFactoryConfigurator>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IReceiveEndpointConfigurator>(MockBehavior.Strict);

            var endpointConfiguratorWasCalled = false;
            busHostBuilder.AddReceiveEndpoint("queue-name-test",
                (IReceiveEndpointBuilder<IHost, IReceiveEndpointConfigurator> builder) =>
                {
                    builder.AddConfigurator((host, endpointConfigurator, serviceProvider) =>
                    {
                        endpointConfiguratorWasCalled = true;
                        Assert.Same(mockHost.Object, host);
                        Assert.Same(mockServiceProvider.Object, serviceProvider);
                        Assert.Same(mockReceiveEndpointConfigurator.Object, endpointConfigurator);
                    });
                });

            var receiveEndpointConfigurator = mockReceiveEndpointConfigurator.Object;

            mockBusFactoryConfigurator
                .Setup(_ => _.ReceiveEndpoint("queue-name-test", It.IsAny<Action<IReceiveEndpointConfigurator>>()))
                .Callback((string queueName, Action<IReceiveEndpointConfigurator> configureEndpoint) =>
                    configureEndpoint(receiveEndpointConfigurator))
                .Verifiable();

            busHostBuilder.DoConfigure(mockHost.Object, mockBusFactoryConfigurator.Object, mockServiceProvider.Object);

            Assert.True(endpointConfiguratorWasCalled);
            mockReceiveEndpointConfigurator.Verify();
            mockBusFactoryConfigurator.Verify();
        }

        /// <summary />
        [Fact]
        public void Initialize_ThenValid()
        {
            var services = new ServiceCollection();

            var busHostBuilder1 = new DummyBusHostBuilder<IHost, IBusFactoryConfigurator>(services, "test1");

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Singleton &&
                item.ServiceType == typeof(IBusHostFactory) &&
                item.ImplementationInstance == busHostBuilder1);

            Assert.Same(services, busHostBuilder1.Services);
            Assert.Equal("test1", busHostBuilder1.ConnectionName);

            var busHostBuilder2 = new DummyBusHostBuilder<IHost, IBusFactoryConfigurator>(services, "test2");

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Singleton &&
                item.ServiceType == typeof(IBusHostFactory) &&
                item.ImplementationInstance == busHostBuilder2);

            Assert.Same(services, busHostBuilder2.Services);
            Assert.Equal("test2", busHostBuilder2.ConnectionName);
        }

        /// <summary />
        [Fact]
        public void UseCorrelationManager_GivenAccessor_ThenValid()
        {
            var services = new ServiceCollection();
            var busHostBuilder = new DummyBusHostBuilder<IHost, IBusFactoryConfigurator>(services, "test");

            busHostBuilder.UseCorrelationManager(context => context.CorrelationId ?? Guid.Empty);

            AssertSpecifications(busHostBuilder);
        }

        /// <summary />
        [Fact]
        public void UseCorrelationManager_ThenValid()
        {
            var services = new ServiceCollection();
            var busHostBuilder = new DummyBusHostBuilder<IHost, IBusFactoryConfigurator>(services, "test");

            busHostBuilder.UseCorrelationManager();

            AssertSpecifications(busHostBuilder);
        }

        /// <summary />
        [Fact]
        public void UseHostAccessor_ThenValid()
        {
            var services = new ServiceCollection();
            var busHostBuilder = new DummyBusHostBuilder<IHost, IBusFactoryConfigurator>(services, "test");

            busHostBuilder.UseHostAccessor();

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Transient &&
                item.ServiceType == typeof(IHostAccessor) &&
                item.ImplementationType == typeof(HostAccessor));

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Singleton &&
                item.ServiceType == typeof(IHostAccessor<IHost>) &&
                ((IHostAccessor<IHost>) item.ImplementationInstance).ConnectionName == "test" &&
                ((IHostAccessor<IHost>) item.ImplementationInstance).Host == null);

            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockBusFactoryConfigurator = new Mock<IBusFactoryConfigurator>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            busHostBuilder.DoConfigure(mockHost.Object, mockBusFactoryConfigurator.Object, mockServiceProvider.Object);

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Singleton &&
                item.ServiceType == typeof(IHostAccessor<IHost>) &&
                ((IHostAccessor<IHost>) item.ImplementationInstance).ConnectionName == "test" &&
                ((IHostAccessor<IHost>) item.ImplementationInstance).Host == mockHost.Object);
        }

        /// <summary />
        [Fact]
        public void UseServiceScope_ThenValid()
        {
            var services = new ServiceCollection();
            var busHostBuilder = new DummyBusHostBuilder<IHost, IBusFactoryConfigurator>(services, "test");

            busHostBuilder.UseServiceScope();

            AssertSpecifications(busHostBuilder);
        }
    }
}