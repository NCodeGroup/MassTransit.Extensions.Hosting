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
    /// Provides an abstraction to configure and initialize receiving endpoints.
    /// </summary>
    public interface IReceiveEndpointBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> Dependency Injection container.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure the receiving endpoint.
        /// </summary>
        /// <param name="endpointConfigurator">The configuration callback to configure the receiving endpoint.</param>
        void AddConfigurator(Action<IHost, IReceiveEndpointConfigurator, IServiceProvider> endpointConfigurator);
    }

    /// <summary>
    /// Provides an abstraction to configure and initialize receiving endpoints.
    /// </summary>
    public interface IReceiveEndpointBuilder<out THost, out TEndpoint> : IReceiveEndpointBuilder
        where THost : class, IHost
        where TEndpoint : class, IReceiveEndpointConfigurator
    {
        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure the receiving endpoint.
        /// </summary>
        /// <param name="endpointConfigurator">The configuration callback to configure the receiving endpoint.</param>
        void AddConfigurator(Action<THost, TEndpoint, IServiceProvider> endpointConfigurator);
    }

    /// <summary>
    /// Provides a common implementation of <see cref="IReceiveEndpointBuilder{THost,TEndpoint}"/>.
    /// </summary>
    /// <typeparam name="THost"><see cref="IHost"/></typeparam>
    /// <typeparam name="TEndpoint"><see cref="IReceiveEndpointConfigurator"/></typeparam>
    public class ReceiveEndpointBuilder<THost, TEndpoint> : IReceiveEndpointBuilder<THost, TEndpoint>
        where THost : class, IHost
        where TEndpoint : class, IReceiveEndpointConfigurator
    {
        private readonly IList<Action<THost, TEndpoint, IServiceProvider>> _configuratorActions =
            new List<Action<THost, TEndpoint, IServiceProvider>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveEndpointBuilder{THost,TEndpoint}"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        public ReceiveEndpointBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <inheritdoc />
        public virtual void AddConfigurator(
            Action<IHost, IReceiveEndpointConfigurator, IServiceProvider> endpointConfigurator)
        {
            if (endpointConfigurator == null)
                throw new ArgumentNullException(nameof(endpointConfigurator));

            _configuratorActions.Add(endpointConfigurator);
        }

        /// <inheritdoc />
        public virtual void AddConfigurator(Action<THost, TEndpoint, IServiceProvider> endpointConfigurator)
        {
            if (endpointConfigurator == null)
                throw new ArgumentNullException(nameof(endpointConfigurator));

            _configuratorActions.Add(endpointConfigurator);
        }

        /// <summary>
        /// Used to invoke all the configuration callbacks when configuring a receiving endpoint.
        /// </summary>
        /// <param name="host"><see cref="IHost"/></param>
        /// <param name="endpoint"><see cref="IReceiveEndpointConfigurator"/></param>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/></param>
        public virtual void Configure(THost host, TEndpoint endpoint, IServiceProvider serviceProvider)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            foreach (var configuratorAction in _configuratorActions)
            {
                configuratorAction(host, endpoint, serviceProvider);
            }
        }
    }
}