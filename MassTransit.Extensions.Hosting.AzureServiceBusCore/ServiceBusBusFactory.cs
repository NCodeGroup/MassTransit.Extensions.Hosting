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

namespace MassTransit.Extensions.Hosting.AzureServiceBusCore
{
    /// <summary>
    /// Provides an implementation of <see cref="IBusFactory{TConfigurator}"/>
    /// that creates AzureServiceBus instances using <see cref="IServiceBusBusFactoryConfigurator"/>.
    /// </summary>
    public class ServiceBusBusFactory : IBusFactory<IServiceBusBusFactoryConfigurator>
    {
        /// <inheritdoc />
        public virtual IBusControl Create(Action<IServiceBusBusFactoryConfigurator> configure)
        {
            return Bus.Factory.CreateUsingAzureServiceBus(configure);
        }

    }
}