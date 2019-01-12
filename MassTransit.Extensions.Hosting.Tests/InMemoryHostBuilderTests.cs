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
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit.Scoping;
using MassTransit.Transports.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace MassTransit.Extensions.Hosting.Tests
{
    /// <summary />
    public class InMemoryHostBuilderTests
    {
        /// <summary />
        public InMemoryHostBuilderTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        private readonly ITestOutputHelper _output;

        private async Task UseServiceScope(bool useServiceScope)
        {
            _output.WriteLine($"UseServiceScope: {useServiceScope}");

            const string connectionName = "connection-name-test";
            const string queueName = "queue-name-test";

            var message = new ExampleMessage
            {
                CorrelationId = Guid.NewGuid(),
                StringData = "FooBar",
                DateTimeOffsetData = DateTimeOffset.Now,
            };

            var services = new ServiceCollection();

            services.AddSingleton(_output);

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXUnit(_output);
            });

            var inlineFilterWasCalled = false;
            var exceptions = new List<ExceptionDispatchInfo>();

            services.AddMassTransit(busBuilder =>
            {
                busBuilder.UseInMemory(connectionName, hostBuilder =>
                {
                    hostBuilder.AddConfigurator(configurator =>
                    {
                        configurator.UseInlineFilter(async (context, next) =>
                        {
                            var inputAddress = context.ReceiveContext.InputAddress;
                            var messageTypes = string.Join(",", context.SupportedMessageTypes);
                            _output.WriteLine($"UseExecute: InputAddress={inputAddress}; MessageTypes={messageTypes}");

                            try
                            {
                                await next.Send(context).ConfigureAwait(false);
                            }
                            catch (Exception exception)
                            {
                                exceptions.Add(ExceptionDispatchInfo.Capture(exception));
                                throw;
                            }
                        });
                    });

                    hostBuilder.UseHostAccessor();

                    if (useServiceScope)
                        hostBuilder.UseServiceScope();

                    hostBuilder.AddConfigurator((host, busFactory, serviceProviderBuilder) =>
                    {
                        busFactory.UseInlineFilter(async (context, next) =>
                        {
                            var hasPayloadInline = context.TryGetPayload<IServiceProvider>(out var serviceProviderInline);
                            Assert.Equal(useServiceScope, hasPayloadInline);
                            if (useServiceScope)
                            {
                                Assert.True(hasPayloadInline);
                                Assert.NotNull(serviceProviderInline);
                                Assert.NotSame(serviceProviderBuilder, serviceProviderInline);
                            }
                            else
                            {
                                Assert.False(hasPayloadInline);
                                Assert.Null(serviceProviderInline);
                            }

                            var consumerServiceProvider = serviceProviderInline ?? serviceProviderBuilder;
                            var consumerScopeProvider = consumerServiceProvider.GetRequiredService<IConsumerScopeProvider>();

                            using (var scope = consumerScopeProvider.GetScope(context))
                            {
                                var hasPayloadScope = scope.Context.TryGetPayload<IServiceProvider>(out var serviceProviderScope);
                                Assert.True(hasPayloadScope);

                                if (useServiceScope)
                                {
                                    Assert.Same(consumerServiceProvider, serviceProviderScope);
                                }
                                else
                                {
                                    Assert.NotSame(consumerServiceProvider, serviceProviderScope);
                                }

                                await next.Send(scope.Context).ConfigureAwait(false);

                                inlineFilterWasCalled = true;
                            }
                        });
                    });

                    hostBuilder.AddReceiveEndpoint(queueName, endpoint =>
                    {
                        endpoint.AddConsumer<ExampleConsumer>(consumerConfigurator =>
                        {
                            consumerConfigurator.Message<IExampleMessage>(messageConfigurator =>
                            {
                                messageConfigurator.UseExecute(context =>
                                {
                                    Assert.Equal(message.CorrelationId, context.Message.CorrelationId);
                                    Assert.Equal(message.StringData, context.Message.StringData);
                                    Assert.Equal(message.DateTimeOffsetData, context.Message.DateTimeOffsetData);
                                });
                            });
                        });
                    });
                });
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busManager = serviceProvider.GetRequiredService<IBusManager>();

                await busManager.StartAsync(CancellationToken.None).ConfigureAwait(false);

                await Task.Yield();

                var hostAccessor = serviceProvider.GetRequiredService<IHostAccessor>();
                var inMemoryHost = hostAccessor.GetHost<IInMemoryHost>(connectionName);
                var bus = busManager.GetBus(connectionName);

                var sendAddress = new Uri(inMemoryHost.Address, queueName);
                var sendEndpoint = await bus.GetSendEndpoint(sendAddress).ConfigureAwait(false);
                await sendEndpoint.Send<IExampleMessage>(message).ConfigureAwait(false);

                await Task.Delay(500).ConfigureAwait(false);

                await busManager.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }

            exceptions.ForEach(ex => ex.Throw());
            Assert.True(inlineFilterWasCalled);
        }

        /// <summary />
        [Fact]
        public void AddReceiveEndpoint_GivenConfigurator_ThenValid()
        {
            var services = new ServiceCollection();
            const string connectionName = "connection-name-test";
            var baseAddress = new Uri("rabbitmq://localhost");

            var inMemoryHostBuilder = new InMemoryHostBuilder(services, connectionName, baseAddress);

            var endpointConfiguratorWasCalled = false;
            inMemoryHostBuilder.AddReceiveEndpoint("queue-name-test", (IInMemoryReceiveEndpointBuilder builder) =>
            {
                Assert.Same(services, builder.Services);

                builder.AddConfigurator((host, endpointConfigurator, serviceProvider) =>
                {
                    endpointConfiguratorWasCalled = true;
                    Assert.Equal(baseAddress, host.Address);
                });
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var busControl = inMemoryHostBuilder.Create(serviceProvider);
                Assert.NotNull(busControl);
                Assert.True(endpointConfiguratorWasCalled);
            }
        }

        /// <summary />
        [Fact]
        public void Create_ThenValid()
        {
            var services = new ServiceCollection();
            const string connectionName = "connection-name-test";
            var baseAddress = new Uri("rabbitmq://localhost");

            var inMemoryHostBuilder = new InMemoryHostBuilder(services, connectionName, baseAddress);

            Assert.Same(services, inMemoryHostBuilder.Services);
            Assert.Equal(connectionName, inMemoryHostBuilder.ConnectionName);

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Singleton &&
                item.ServiceType == typeof(IBusHostFactory) &&
                item.ImplementationInstance == inMemoryHostBuilder);

            inMemoryHostBuilder.UseHostAccessor();

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var hostAccessor = serviceProvider.GetRequiredService<IHostAccessor>();

                var inMemoryHostBefore = hostAccessor.GetHost<IInMemoryHost>(connectionName);
                Assert.Null(inMemoryHostBefore);

                var busControl = inMemoryHostBuilder.Create(serviceProvider);
                Assert.NotNull(busControl);

                var inMemoryHostAfter = hostAccessor.GetHost<IInMemoryHost>(connectionName);
                Assert.NotNull(inMemoryHostAfter);
                Assert.Equal(baseAddress, inMemoryHostAfter.Address);
            }
        }

        /// <summary />
        [Fact]
        public async Task UseServiceScope_ThenValid()
        {
            await UseServiceScope(true).ConfigureAwait(false);
            _output.WriteLine(string.Empty);
            await UseServiceScope(false).ConfigureAwait(false);
        }

    }
}