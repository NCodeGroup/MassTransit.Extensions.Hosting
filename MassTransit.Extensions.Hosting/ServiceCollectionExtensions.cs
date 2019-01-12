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
using System.Linq;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Scoping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures the services required for using <c>MassTransit</c>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used to configure the DI container.</param>
        /// <param name="configurator">The configuration callback to configure the Bus.</param>
        /// <returns><see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddMassTransit(this IServiceCollection services,
            Action<IMassTransitBuilder> configurator)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var builder = new MassTransitBuilder(services);
            configurator(builder);

            var busManagerDescriptor = ServiceDescriptor.Singleton<IBusManager, BusManager>();
            if (services.All(item => item.ServiceType != busManagerDescriptor.ServiceType))
            {
                services.Add(busManagerDescriptor);
                services.AddSingleton<IHostedService>(serviceProvider =>
                    serviceProvider.GetRequiredService<IBusManager>());
            }

            services.TryAddTransient<IConsumerScopeProvider, DependencyInjectionConsumerScopeProvider>();
            services.TryAdd(ServiceDescriptor.Transient(typeof(IConsumerFactory<>), typeof(ScopeConsumerFactory<>)));

            return services;
        }
    }
}