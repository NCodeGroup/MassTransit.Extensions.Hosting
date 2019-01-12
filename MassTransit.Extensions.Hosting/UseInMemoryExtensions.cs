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

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides extension methods for <see cref="IMassTransitBuilder"/> to configure in-memory bus instances.
    /// </summary>
    public static class UseInMemoryExtensions
    {
        /// <summary>
        /// Configures an in-memory bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the InMemory bus.</param>
        public static void UseInMemory(this IMassTransitBuilder builder, string connectionName, Action<IInMemoryHostBuilder> hostConfigurator)
        {
            UseInMemory(builder, connectionName, null, hostConfigurator);
        }

        /// <summary>
        /// Configures an in-memory bus by using the specified base address.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="baseAddress">Contains an optional override for the default base address.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the InMemory bus.</param>
        public static void UseInMemory(this IMassTransitBuilder builder, string connectionName, Uri baseAddress, Action<IInMemoryHostBuilder> hostConfigurator)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var hostBuilder = new InMemoryHostBuilder(builder.Services, connectionName, baseAddress);
            hostConfigurator?.Invoke(hostBuilder);
        }

    }
}