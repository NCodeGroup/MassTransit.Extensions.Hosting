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
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.Extensions.Hosting.Tests
{
    /// <summary />
    public class DummyBusHostBuilder<THost, TBusFactory> : BusHostBuilder<THost, TBusFactory>
        where THost : class, IHost
        where TBusFactory : class, IBusFactoryConfigurator
    {
        /// <summary />
        public DummyBusHostBuilder(IServiceCollection services, string connectionName)
            : base(services, connectionName)
        {
            // nothing
        }

        /// <summary />
        public Func<IServiceProvider, IBusControl> CreateFunc { get; set; }

        /// <summary />
        public void DoConfigure(THost host, TBusFactory busFactory, IServiceProvider serviceProvider)
        {
            Configure(host, busFactory, serviceProvider);
        }

        /// <summary />
        public override IBusControl Create(IServiceProvider serviceProvider)
        {
            return CreateFunc?.Invoke(serviceProvider);
        }

    }
}