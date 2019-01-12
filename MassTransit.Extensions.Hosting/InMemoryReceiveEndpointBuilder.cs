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

using MassTransit.Transports.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides an abstraction to configure and initialize InMemory specific receiving endpoints.
    /// </summary>
    public interface
        IInMemoryReceiveEndpointBuilder : IReceiveEndpointBuilder<IInMemoryHost, IInMemoryReceiveEndpointConfigurator>
    {
        // nothing
    }

    /// <summary>
    /// Provides the implementation for <see cref="IInMemoryReceiveEndpointBuilder"/>.
    /// </summary>
    public class InMemoryReceiveEndpointBuilder :
        ReceiveEndpointBuilder<IInMemoryHost, IInMemoryReceiveEndpointConfigurator>,
        IInMemoryReceiveEndpointBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryReceiveEndpointBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        public InMemoryReceiveEndpointBuilder(IServiceCollection services)
            : base(services)
        {
            // nothing
        }
    }
}