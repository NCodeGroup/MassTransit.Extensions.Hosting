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
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides the ability to retrieve <see cref="IHost"/> after startup.
    /// </summary>
    public interface IHostAccessor
    {
        /// <summary>
        /// Returns the <see cref="IHost"/> for the specified connection name.
        /// </summary>
        /// <typeparam name="THost">The type of <see cref="IHost"/>.</typeparam>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <returns><see cref="IHost"/></returns>
        THost GetHost<THost>(string connectionName)
            where THost : IHost;
    }

    /// <summary>
    /// Provides the ability to retrieve <see cref="IHost"/> after startup.
    /// </summary>
    /// <typeparam name="THost">The type of <see cref="IHost"/>.</typeparam>
    public interface IHostAccessor<out THost>
        where THost : IHost
    {
        /// <summary>
        /// Gets the client-provided connection name.
        /// </summary>
        string ConnectionName { get; }

        /// <summary>
        /// Gets or sets the <see cref="IHost"/>.
        /// </summary>
        THost Host { get; }
    }

    /// <summary>
    /// Provides the implementation for <see cref="IHostAccessor"/>.
    /// </summary>
    public class HostAccessor : IHostAccessor
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostAccessor"/> class.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/></param>
        public HostAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public virtual THost GetHost<THost>(string connectionName)
            where THost : IHost
        {
            connectionName = connectionName ?? string.Empty;

            return _serviceProvider
                .GetServices<IHostAccessor<THost>>()
                .Where(_ => _.ConnectionName == connectionName)
                .Select(_ => _.Host)
                .FirstOrDefault();
        }
    }

    /// <summary>
    /// Provides the implementation for <see cref="IHostAccessor{THost}"/>.
    /// </summary>
    /// <typeparam name="THost">The type of <see cref="IHost"/>.</typeparam>
    public class HostAccessor<THost> : IHostAccessor<THost>
        where THost : IHost
    {
        /// <inheritdoc />
        public string ConnectionName { get; set; }

        /// <inheritdoc />
        public THost Host { get; set; }
    }
}