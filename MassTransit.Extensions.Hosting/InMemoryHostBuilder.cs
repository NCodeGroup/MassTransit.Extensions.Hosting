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
using MassTransit.Transports.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides an abstraction to configure and initialize in-memory specific Bus instances.
    /// </summary>
    public interface IInMemoryHostBuilder : IBusHostBuilder<IInMemoryHost, IInMemoryBusFactoryConfigurator>
    {
        // nothing
    }

    /// <summary>
    /// Provides an implementation of <see cref="IInMemoryHostBuilder"/>.
    /// </summary>
    public class InMemoryHostBuilder :
        BusHostBuilder<IInMemoryHost, IInMemoryBusFactoryConfigurator>,
        IInMemoryHostBuilder
    {
        private readonly Uri _baseAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="baseAddress">Contains an optional override for the default base address.</param>
        public InMemoryHostBuilder(IServiceCollection services, string connectionName, Uri baseAddress)
            : base(services, connectionName)
        {
            _baseAddress = baseAddress;

            services.TryAddTransient<IInMemoryBusFactory, InMemoryBusFactory>();
            services.TryAddTransient<IBusFactory<IInMemoryBusFactoryConfigurator>, InMemoryBusFactory>();
        }

        /// <inheritdoc />
        public override IBusControl Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger<InMemoryHostBuilder>();
            var loggerIsEnabled = logger?.IsEnabled(LogLevel.Debug) ?? false;
            if (loggerIsEnabled)
                logger.LogDebug("Creating InMemory Bus '{0}'", ConnectionName);

            var busFactory = serviceProvider.GetRequiredService<IInMemoryBusFactory>();
            var busControl = busFactory.Create(_baseAddress, busFactoryConfigurator =>
            {
                Configure(busFactoryConfigurator.Host, busFactoryConfigurator, serviceProvider);
            });

            if (loggerIsEnabled)
                logger.LogDebug("Created InMemory Bus '{0}'", ConnectionName);

            return busControl;
        }

    }
}