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
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;

namespace MassTransit.Extensions.Hosting.AzureServiceBus
{
    /// <summary>
    /// Contains the options for configuring an AzureServiceBus host.
    /// </summary>
    public class ServiceBusOptions
    {
        /// <summary>
        /// Gets or sets the client-provided connection name.
        /// </summary>
        public string ConnectionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the service bus namespace and accompanying service scope in MassTransit format (sb://namespace.servicebus.windows.net/scope).
        /// </summary>
        public Uri HostAddress { get; set; }

        /// <summary>
        /// Gets or sets the token provider to access the namespace.
        /// </summary>
        public ITokenProvider TokenProvider { get; set; }

        /// <summary>
        /// Gets or sets the operation timeout for the messaging factory, defaults to 60s.
        /// </summary>
        public TimeSpan? OperationTimeout { get; set; }

        /// <summary>
        /// Gets or sets the minimum back off interval for the exponential retry policy, defaults to 100ms.
        /// </summary>
        public TimeSpan? RetryMinBackoff { get; set; }

        /// <summary>
        /// Gets or sets the maximum back off interval for the exponential retry policy, defaults to 30s.
        /// </summary>
        public TimeSpan? RetryMaxBackoff { get; set; }

        /// <summary>
        /// Gets or sets the retry limit for service bus operations, defaults to 10.
        /// </summary>
        public int? RetryLimit { get; set; }

        /// <summary>
        /// Gets or sets the messaging protocol to use with the messaging factory, defaults to AMQP.
        /// </summary>
        public TransportType? TransportType { get; set; }
    }
}