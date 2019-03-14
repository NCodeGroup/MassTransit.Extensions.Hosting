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
using System.Diagnostics;
using GreenPipes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides extension methods for <see cref="IBusHostBuilder{THost,TBusFactory}"/>.
    /// </summary>
    public static class BusHostBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="IHostAccessor{THost}"/> to the services container
        /// so that <see cref="IHost"/> may be retrieved after startup.
        /// </summary>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        public static void UseHostAccessor<THost, TBusFactory>(this IBusHostBuilder<THost, TBusFactory> builder)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var hostAccessor = new HostAccessor<THost>
            {
                ConnectionName = builder.ConnectionName,
            };

            builder.Services.AddSingleton<IHostAccessor<THost>>(hostAccessor);
            builder.Services.TryAddTransient<IHostAccessor, HostAccessor>();

            builder.AddConfigurator((host, busFactory, serviceProvider) =>
            {
                hostAccessor.Host = host;
            });
        }

        /// <summary>
        /// Creates a scope which is used by all downstream filters/consumers/etc.
        /// </summary>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        public static void UseServiceScope<THost, TBusFactory>(this IBusHostBuilder<THost, TBusFactory> builder)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.AddConfigurator((host, busFactory, serviceProvider) =>
            {
                busFactory.UseServiceScope(serviceProvider);
            });
        }

        /// <summary>
        /// Connects an observer to the bus, to observe creation, start, stop, fault events.
        /// </summary>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        /// <param name="observer"><see cref="IBusObserver"/></param>
        public static void ConnectBusObserver<THost, TBusFactory>(this IBusHostBuilder<THost, TBusFactory> builder, IBusObserver observer)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
        {
            builder.AddConfigurator((host, busFactory, serviceProvider) =>
            {
                busFactory.BusObserver(observer);
            });
        }

        /// <summary>
        /// Connects an observer(s) to the bus, to observe creation, start,
        /// stop, fault events. The observer is resolved from the Dependency
        /// Injection container. If multiple registrations are found, then all
        /// are used.
        /// </summary>
        /// <typeparam name="THost">The type of <see cref="IHost"/>.</typeparam>
        /// <typeparam name="TBusFactory">The type of <see cref="IBusFactoryConfigurator"/>.</typeparam>
        /// <typeparam name="TBusObserver">The type of <see cref="IBusObserver"/>.</typeparam>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        public static void ConnectBusObservers<THost, TBusFactory, TBusObserver>(this IBusHostBuilder<THost, TBusFactory> builder)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
            where TBusObserver : class, IBusObserver
        {
            builder.AddConfigurator((host, busFactory, serviceProvider) =>
            {
                var busObservers = serviceProvider.GetServices<TBusObserver>();
                foreach (var busObserver in busObservers)
                {
                    busFactory.BusObserver(busObserver);
                }
            });
        }

        /// <summary>
        /// Connects multiple observers to the bus, to observe creation, start,
        /// stop, fault events. The observers are resolved from the Dependency
        /// Injection container
        /// </summary>
        /// <typeparam name="THost">The type of <see cref="IHost"/>.</typeparam>
        /// <typeparam name="TBusFactory">The type of <see cref="IBusFactoryConfigurator"/>.</typeparam>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        public static void ConnectBusObservers<THost, TBusFactory>(this IBusHostBuilder<THost, TBusFactory> builder)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
        {
            ConnectBusObservers<THost, TBusFactory, IBusObserver>(builder);
        }

        /// <summary>
        /// Assigns <see cref="P:Trace.CorrelationManager.ActivityId"/> from the CorrelationId on the <see cref="ConsumeContext"/>.
        /// </summary>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        public static void UseCorrelationManager<THost, TBusFactory>(this IBusHostBuilder<THost, TBusFactory> builder)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
        {
            UseCorrelationManager(builder, context => context.CorrelationId ?? NewId.NextGuid());
        }

        /// <summary>
        /// Assigns <see cref="P:Trace.CorrelationManager.ActivityId"/> from the CorrelationId on the <see cref="ConsumeContext"/>.
        /// </summary>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        /// <param name="correlationIdAccessor">The callback used to return the <c>CorrelationId</c> from <see cref="ConsumeContext"/>.</param>
        public static void UseCorrelationManager<THost, TBusFactory>(this IBusHostBuilder<THost, TBusFactory> builder, Func<ConsumeContext, Guid> correlationIdAccessor)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (correlationIdAccessor == null)
                throw new ArgumentNullException(nameof(correlationIdAccessor));

            builder.AddConfigurator((host, busFactory, serviceProvider) =>
            {
                busFactory.UseExecute(context =>
                {
                    var correlationId = correlationIdAccessor(context);
                    Trace.CorrelationManager.ActivityId = correlationId;
                });
            });
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure the Bus.
        /// </summary>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        /// <param name="busFactoryConfigurator">The configuration callback to configure the Bus.</param>
        public static void AddConfigurator<THost, TBusFactory>(this IBusHostBuilder<THost, TBusFactory> builder, Action<TBusFactory, IServiceProvider> busFactoryConfigurator)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (busFactoryConfigurator == null)
                throw new ArgumentNullException(nameof(busFactoryConfigurator));

            builder.AddConfigurator((host, busFactory, serviceProvider) => busFactoryConfigurator(busFactory, serviceProvider));
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure the Bus.
        /// </summary>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        /// <param name="busFactoryConfigurator">The configuration callback to configure the Bus.</param>
        public static void AddConfigurator<THost, TBusFactory>(this IBusHostBuilder<THost, TBusFactory> builder, Action<IHost, TBusFactory> busFactoryConfigurator)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (busFactoryConfigurator == null)
                throw new ArgumentNullException(nameof(busFactoryConfigurator));

            builder.AddConfigurator((host, busFactory, serviceProvider) => busFactoryConfigurator(host, busFactory));
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure the Bus.
        /// </summary>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        /// <param name="busFactoryConfigurator">The configuration callback to configure the Bus.</param>
        public static void AddConfigurator<THost, TBusFactory>(this IBusHostBuilder<THost, TBusFactory> builder, Action<TBusFactory> busFactoryConfigurator)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (busFactoryConfigurator == null)
                throw new ArgumentNullException(nameof(busFactoryConfigurator));

            builder.AddConfigurator((host, busFactory, serviceProvider) => busFactoryConfigurator(busFactory));
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure
        /// a receiving endpoint for the Bus with the specified queue name.
        /// </summary>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        /// <param name="queueName">The queue name for the receiving endpoint.</param>
        /// <param name="endpointConfigurator">The configuration callback to configure the receiving endpoint.</param>
        public static void AddReceiveEndpoint<THost, TBusFactory>(this IBusHostBuilder<THost, TBusFactory> builder, string queueName, Action<IReceiveEndpointBuilder> endpointConfigurator)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
        {
            AddReceiveEndpoint(builder, queueName, (IReceiveEndpointBuilder<THost, IReceiveEndpointConfigurator> endpointBuilder) => endpointConfigurator?.Invoke(endpointBuilder));
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure
        /// a receiving endpoint for the Bus with the specified queue name.
        /// </summary>
        /// <param name="builder"><see cref="IBusHostBuilder{THost,TBusFactory}"/></param>
        /// <param name="queueName">The queue name for the receiving endpoint.</param>
        /// <param name="endpointConfigurator">The configuration callback to configure the receiving endpoint.</param>
        public static void AddReceiveEndpoint<THost, TBusFactory>(this IBusHostBuilder<THost, TBusFactory> builder, string queueName, Action<IReceiveEndpointBuilder<THost, IReceiveEndpointConfigurator>> endpointConfigurator)
            where THost : class, IHost
            where TBusFactory : class, IBusFactoryConfigurator
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (queueName == null)
                throw new ArgumentNullException(nameof(queueName));

            var endpointBuilder = new ReceiveEndpointBuilder<THost, IReceiveEndpointConfigurator>(builder.Services);
            endpointConfigurator?.Invoke(endpointBuilder);

            builder.AddConfigurator((host, busFactory, serviceProvider) =>
            {
                busFactory.ReceiveEndpoint(queueName, endpoint =>
                {
                    endpointBuilder.Configure(host, endpoint, serviceProvider);
                });
            });
        }

    }
}