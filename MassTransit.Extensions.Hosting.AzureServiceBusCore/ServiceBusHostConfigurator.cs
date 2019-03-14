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
using MassTransit.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;

namespace MassTransit.Extensions.Hosting.AzureServiceBusCore
{
    internal class ServiceBusHostConfigurator : IServiceBusHostConfigurator, ServiceBusHostSettings
    {
        public ServiceBusHostConfigurator(Uri hostAddress)
        {
            ServiceUri = hostAddress ?? throw new ArgumentNullException(nameof(hostAddress));
        }

        public ServiceBusHostSettings Settings => this;

        public Uri ServiceUri { get; }

        public ITokenProvider TokenProvider { get; set; }

        public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(60);

        public TimeSpan RetryMinBackoff { get; set; } = TimeSpan.FromMilliseconds(100);

        public TimeSpan RetryMaxBackoff { get; set; } = TimeSpan.FromSeconds(30);

        public int RetryLimit { get; set; } = 10;

        public TransportType TransportType { get; set; } = TransportType.Amqp;
    }
}