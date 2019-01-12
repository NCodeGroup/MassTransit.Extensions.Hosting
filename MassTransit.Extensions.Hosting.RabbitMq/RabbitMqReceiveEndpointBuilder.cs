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

using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.Extensions.Hosting.RabbitMq
{
    /// <summary>
    /// Provides an abstraction to configure and initialize RabbitMq specific receiving endpoints.
    /// </summary>
    public interface IRabbitMqReceiveEndpointBuilder : IReceiveEndpointBuilder<IRabbitMqHost, IRabbitMqReceiveEndpointConfigurator>
    {
        // nothing
    }

    /// <summary>
    /// Provides an implementation of <see cref="IRabbitMqReceiveEndpointBuilder"/>.
    /// </summary>
    public class RabbitMqReceiveEndpointBuilder :
        ReceiveEndpointBuilder<IRabbitMqHost, IRabbitMqReceiveEndpointConfigurator>,
        IRabbitMqReceiveEndpointBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqReceiveEndpointBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        public RabbitMqReceiveEndpointBuilder(IServiceCollection services)
            : base(services)
        {
            // nothing
        }

    }
}