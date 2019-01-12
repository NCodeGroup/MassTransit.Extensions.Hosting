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
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides an abstraction to create Bus instances.
    /// </summary>
    public interface IBusHostFactory
    {
        /// <summary>
        /// Gets the client-provided connection name.
        /// </summary>
        string ConnectionName { get; }

        /// <summary>
        /// Creates and returns a new Bus instance.
        /// </summary>
        /// <param name="serviceProvider">Used to resolve any dependencies from the Dependency Injection container.</param>
        /// <returns><see cref="IBusControl"/></returns>
        IBusControl Create(IServiceProvider serviceProvider);
    }

    /// <summary>
    /// Provides an abstraction to configure and initialize Bus instances.
    /// </summary>
    public interface IBusHostBuilder<out THost, out TBusFactory> : IBusHostFactory
        where THost : class, IHost
        where TBusFactory : class, IBusFactoryConfigurator
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> Dependency Injection container.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure the Bus.
        /// </summary>
        /// <param name="busFactoryConfigurator">The configuration callback to configure the Bus.</param>
        void AddConfigurator(Action<THost, TBusFactory, IServiceProvider> busFactoryConfigurator);
    }

    /// <summary>
    /// Provides a common implementation for <see cref="IBusHostBuilder{THost,TBusFactory}"/>.
    /// </summary>
    /// <typeparam name="THost">The type of <see cref="IHost"/>.</typeparam>
    /// <typeparam name="TBusFactory">The type of <see cref="IBusFactoryConfigurator"/>.</typeparam>
    public abstract class BusHostBuilder<THost, TBusFactory> : IBusHostBuilder<THost, TBusFactory>
        where THost : class, IHost
        where TBusFactory : class, IBusFactoryConfigurator
    {
        private readonly IList<Action<THost, TBusFactory, IServiceProvider>> _configuratorActions = new List<Action<THost, TBusFactory, IServiceProvider>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BusHostBuilder{THost,TBusFactory}"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        protected BusHostBuilder(IServiceCollection services, string connectionName)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            ConnectionName = connectionName ?? string.Empty;

            services.AddSingleton<IBusHostFactory>(this);
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <inheritdoc />
        public string ConnectionName { get; }

        /// <inheritdoc />
        public virtual void AddConfigurator(Action<THost, TBusFactory, IServiceProvider> busFactoryConfigurator)
        {
            if (busFactoryConfigurator == null)
                throw new ArgumentNullException(nameof(busFactoryConfigurator));

            _configuratorActions.Add(busFactoryConfigurator);
        }

        /// <inheritdoc />
        public abstract IBusControl Create(IServiceProvider serviceProvider);

        /// <summary>
        /// Used to invoke all the configuration callbacks when configuring a bus instance.
        /// </summary>
        /// <param name="host"><see cref="IHost"/></param>
        /// <param name="busFactory"><see cref="IBusFactoryConfigurator"/></param>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/></param>
        protected void Configure(THost host, TBusFactory busFactory, IServiceProvider serviceProvider)
        {
            foreach (var configuratorAction in _configuratorActions)
            {
                configuratorAction(host, busFactory, serviceProvider);
            }
        }

    }
}