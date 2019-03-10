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

namespace MassTransit.Extensions.Hosting.ActiveMq
{
    /// <summary>
    /// Provides extension methods for <see cref="IActiveMqHostBuilder"/>.
    /// </summary>
    public static class ActiveMqHostBuilderExtensions
    {
        /// <summary>
        /// Sets the username and password for the connection to ActiveMQ.
        /// </summary>
        /// <param name="builder"><see cref="IActiveMqHostBuilder"/></param>
        /// <param name="username">Contains the username.</param>
        /// <param name="password">Contains the password.</param>
        public static IActiveMqHostBuilder UseCredentials(this IActiveMqHostBuilder builder, string username, string password)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.UseUsername(username);
            builder.UsePassword(password);

            return builder;
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure
        /// a receiving endpoint for the Bus with the specified queue name.
        /// </summary>
        /// <param name="builder"><see cref="IActiveMqHostBuilder"/></param>
        /// <param name="queueName">The queue name for the receiving endpoint.</param>
        /// <param name="endpointConfigurator">The configuration callback to configure the receiving endpoint.</param>
        public static void AddReceiveEndpoint(this IActiveMqHostBuilder builder, string queueName, Action<IActiveMqReceiveEndpointBuilder> endpointConfigurator)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (queueName == null)
                throw new ArgumentNullException(nameof(queueName));

            var endpointBuilder = new ActiveMqReceiveEndpointBuilder(builder.Services);
            endpointConfigurator?.Invoke(endpointBuilder);

            builder.AddConfigurator((host, busFactory, serviceProvider) =>
            {
                busFactory.ReceiveEndpoint(host, queueName, endpoint =>
                {
                    endpointBuilder.Configure(host, endpoint, serviceProvider);
                });
            });
        }

    }
}