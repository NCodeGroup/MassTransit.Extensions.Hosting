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
using MassTransit.ConsumeConfigurators;
using MassTransit.Transports.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MassTransit.Extensions.Hosting.Tests
{
    /// <summary />
    public class ReceiveEndpointBuilderTests
    {
        /// <summary />
        [Fact]
        public void AddConfigurator_GivenCovariantEndpoint_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder =
                new ReceiveEndpointBuilder<IHost, IInMemoryReceiveEndpointConfigurator>(services);

            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IInMemoryReceiveEndpointConfigurator>(MockBehavior.Strict);

            var configuratorWasCalled = false;
            receiveEndpointBuilder.AddConfigurator((IHost host, IReceiveEndpointConfigurator configurator) =>
            {
                configuratorWasCalled = true;
                Assert.Same(mockHost.Object, host);
                Assert.Same(mockReceiveEndpointConfigurator.Object, configurator);
            });

            receiveEndpointBuilder.Configure(mockHost.Object, mockReceiveEndpointConfigurator.Object,
                mockServiceProvider.Object);

            Assert.True(configuratorWasCalled);
        }

        /// <summary />
        [Fact]
        public void AddConfigurator_GivenCovariantHost_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder =
                new ReceiveEndpointBuilder<IInMemoryHost, IReceiveEndpointConfigurator>(services);

            var mockHost = new Mock<IInMemoryHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IReceiveEndpointConfigurator>(MockBehavior.Strict);

            var configuratorWasCalled = false;
            receiveEndpointBuilder.AddConfigurator((IHost host, IReceiveEndpointConfigurator configurator) =>
            {
                configuratorWasCalled = true;
                Assert.Same(mockHost.Object, host);
                Assert.Same(mockReceiveEndpointConfigurator.Object, configurator);
            });

            receiveEndpointBuilder.Configure(mockHost.Object, mockReceiveEndpointConfigurator.Object,
                mockServiceProvider.Object);

            Assert.True(configuratorWasCalled);
        }

        /// <summary />
        [Fact]
        public void AddConfigurator_GivenCovariantHostEndpoint_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder =
                new ReceiveEndpointBuilder<IInMemoryHost, IInMemoryReceiveEndpointConfigurator>(services);

            var mockHost = new Mock<IInMemoryHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IInMemoryReceiveEndpointConfigurator>(MockBehavior.Strict);

            var configuratorWasCalled = false;
            receiveEndpointBuilder.AddConfigurator((IHost host, IReceiveEndpointConfigurator configurator) =>
            {
                configuratorWasCalled = true;
                Assert.Same(mockHost.Object, host);
                Assert.Same(mockReceiveEndpointConfigurator.Object, configurator);
            });

            receiveEndpointBuilder.Configure(mockHost.Object, mockReceiveEndpointConfigurator.Object,
                mockServiceProvider.Object);

            Assert.True(configuratorWasCalled);
        }

        /// <summary />
        [Fact]
        public void AddConfigurator_GivenGeneric_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder =
                new ReceiveEndpointBuilder<IInMemoryHost, IInMemoryReceiveEndpointConfigurator>(services);

            var mockHost = new Mock<IInMemoryHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IInMemoryReceiveEndpointConfigurator>(MockBehavior.Strict);

            var configuratorWasCalled = false;
            IReceiveEndpointBuilder<IInMemoryHost, IInMemoryReceiveEndpointConfigurator> receiveEndpointBuilderGeneric =
                receiveEndpointBuilder;
            receiveEndpointBuilderGeneric.AddConfigurator((host, configurator, serviceProvider) =>
            {
                configuratorWasCalled = true;
                Assert.Same(mockHost.Object, host);
                Assert.Same(mockServiceProvider.Object, serviceProvider);
                Assert.Same(mockReceiveEndpointConfigurator.Object, configurator);
            });

            receiveEndpointBuilder.Configure(mockHost.Object, mockReceiveEndpointConfigurator.Object,
                mockServiceProvider.Object);

            Assert.True(configuratorWasCalled);
        }

        /// <summary />
        [Fact]
        public void AddConfigurator_GivenNonGeneric_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder = new ReceiveEndpointBuilder<IHost, IReceiveEndpointConfigurator>(services);

            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IReceiveEndpointConfigurator>(MockBehavior.Strict);

            var configuratorWasCalled = false;
            IReceiveEndpointBuilder receiveEndpointBuilderNonGeneric = receiveEndpointBuilder;
            receiveEndpointBuilderNonGeneric.AddConfigurator((host, configurator, serviceProvider) =>
            {
                configuratorWasCalled = true;
                Assert.Same(mockHost.Object, host);
                Assert.Same(mockServiceProvider.Object, serviceProvider);
                Assert.Same(mockReceiveEndpointConfigurator.Object, configurator);
            });

            receiveEndpointBuilder.Configure(mockHost.Object, mockReceiveEndpointConfigurator.Object,
                mockServiceProvider.Object);

            Assert.True(configuratorWasCalled);
        }

        /// <summary />
        [Fact]
        public void AddConfigurator_GivenVariantEndpoint_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder = new ReceiveEndpointBuilder<IHost, IReceiveEndpointConfigurator>(services);

            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IReceiveEndpointConfigurator>(MockBehavior.Strict);

            var configuratorWasCalled = false;
            receiveEndpointBuilder.AddConfigurator(configurator =>
            {
                configuratorWasCalled = true;
                Assert.Same(mockReceiveEndpointConfigurator.Object, configurator);
            });

            receiveEndpointBuilder.Configure(mockHost.Object, mockReceiveEndpointConfigurator.Object,
                mockServiceProvider.Object);

            Assert.True(configuratorWasCalled);
        }

        /// <summary />
        [Fact]
        public void AddConfigurator_GivenVariantEndpointServiceProvider_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder = new ReceiveEndpointBuilder<IHost, IReceiveEndpointConfigurator>(services);

            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IReceiveEndpointConfigurator>(MockBehavior.Strict);

            var configuratorWasCalled = false;
            receiveEndpointBuilder.AddConfigurator(
                (IReceiveEndpointConfigurator configurator, IServiceProvider serviceProvider) =>
                {
                    configuratorWasCalled = true;
                    Assert.Same(mockServiceProvider.Object, serviceProvider);
                    Assert.Same(mockReceiveEndpointConfigurator.Object, configurator);
                });

            receiveEndpointBuilder.Configure(mockHost.Object, mockReceiveEndpointConfigurator.Object,
                mockServiceProvider.Object);

            Assert.True(configuratorWasCalled);
        }

        /// <summary />
        [Fact]
        public void AddConfigurator_GivenVariantHostEndpoint_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder = new ReceiveEndpointBuilder<IHost, IReceiveEndpointConfigurator>(services);

            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IReceiveEndpointConfigurator>(MockBehavior.Strict);

            var configuratorWasCalled = false;
            receiveEndpointBuilder.AddConfigurator((IHost host, IReceiveEndpointConfigurator configurator) =>
            {
                configuratorWasCalled = true;
                Assert.Same(mockHost.Object, host);
                Assert.Same(mockReceiveEndpointConfigurator.Object, configurator);
            });

            receiveEndpointBuilder.Configure(mockHost.Object, mockReceiveEndpointConfigurator.Object,
                mockServiceProvider.Object);

            Assert.True(configuratorWasCalled);
        }

        /// <summary />
        [Fact]
        public void AddConsumer_GivenConfigurator_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder = new ReceiveEndpointBuilder<IHost, IReceiveEndpointConfigurator>(services);

            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IReceiveEndpointConfigurator>(MockBehavior.Strict);
            var mockConsumerFactory = new Mock<IConsumerFactory<IConsumer>>(MockBehavior.Strict);

            mockServiceProvider
                .Setup(_ => _.GetService(typeof(IConsumerFactory<IConsumer>)))
                .Returns(mockConsumerFactory.Object)
                .Verifiable();

            mockReceiveEndpointConfigurator
                .Setup(_ => _.AddEndpointSpecification(It.IsAny<ConsumerConfigurator<IConsumer>>()))
                .Verifiable();

            var addConsumerWasCalled = false;
            receiveEndpointBuilder.AddConsumer<IConsumer>(configurator => { addConsumerWasCalled = true; });

            receiveEndpointBuilder.Configure(mockHost.Object, mockReceiveEndpointConfigurator.Object,
                mockServiceProvider.Object);

            Assert.True(addConsumerWasCalled);
            mockServiceProvider.Verify();
            mockReceiveEndpointConfigurator.Verify();
        }

        /// <summary />
        [Fact]
        public void AddConsumer_GivenConfiguratorServiceProvider_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder = new ReceiveEndpointBuilder<IHost, IReceiveEndpointConfigurator>(services);

            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IReceiveEndpointConfigurator>(MockBehavior.Strict);
            var mockConsumerFactory = new Mock<IConsumerFactory<IConsumer>>(MockBehavior.Strict);

            mockServiceProvider
                .Setup(_ => _.GetService(typeof(IConsumerFactory<IConsumer>)))
                .Returns(mockConsumerFactory.Object)
                .Verifiable();

            mockReceiveEndpointConfigurator
                .Setup(_ => _.AddEndpointSpecification(It.IsAny<ConsumerConfigurator<IConsumer>>()))
                .Verifiable();

            var addConsumerWasCalled = false;
            receiveEndpointBuilder.AddConsumer<IConsumer>((configurator, serviceProvider) =>
            {
                addConsumerWasCalled = true;
                Assert.Same(mockServiceProvider.Object, serviceProvider);
            });

            receiveEndpointBuilder.Configure(mockHost.Object, mockReceiveEndpointConfigurator.Object,
                mockServiceProvider.Object);

            Assert.True(addConsumerWasCalled);
            mockServiceProvider.Verify();
            mockReceiveEndpointConfigurator.Verify();
        }

        /// <summary />
        [Fact]
        public void AddConsumer_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder = new ReceiveEndpointBuilder<IHost, IReceiveEndpointConfigurator>(services);

            var mockHost = new Mock<IHost>(MockBehavior.Strict);
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockReceiveEndpointConfigurator = new Mock<IReceiveEndpointConfigurator>(MockBehavior.Strict);
            var mockConsumerFactory = new Mock<IConsumerFactory<IConsumer>>(MockBehavior.Strict);

            mockServiceProvider
                .Setup(_ => _.GetService(typeof(IConsumerFactory<IConsumer>)))
                .Returns(mockConsumerFactory.Object)
                .Verifiable();

            mockReceiveEndpointConfigurator
                .Setup(_ => _.AddEndpointSpecification(It.IsAny<ConsumerConfigurator<IConsumer>>()))
                .Verifiable();

            receiveEndpointBuilder.AddConsumer<IConsumer>();

            receiveEndpointBuilder.Configure(mockHost.Object, mockReceiveEndpointConfigurator.Object,
                mockServiceProvider.Object);

            mockServiceProvider.Verify();
            mockReceiveEndpointConfigurator.Verify();
        }

        /// <summary />
        [Fact]
        public void Initialize_ThenValid()
        {
            var services = new ServiceCollection();
            var receiveEndpointBuilder = new ReceiveEndpointBuilder<IHost, IReceiveEndpointConfigurator>(services);

            Assert.Same(services, receiveEndpointBuilder.Services);
        }
    }
}