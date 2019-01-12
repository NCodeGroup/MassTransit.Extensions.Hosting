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
using MassTransit.RabbitMqTransport;

namespace MassTransit.Extensions.Hosting.RabbitMq
{
    /// <summary>
    /// Provides an implementation of <see cref="IBusFactory{TConfigurator}"/>
    /// that creates RabbitMQ bus instances using <see cref="IRabbitMqBusFactoryConfigurator"/>.
    /// </summary>
    public class RabbitMqBusFactory : IBusFactory<IRabbitMqBusFactoryConfigurator>
    {
        /// <inheritdoc />
        public virtual IBusControl Create(Action<IRabbitMqBusFactoryConfigurator> configure)
        {
            return Bus.Factory.CreateUsingRabbitMq(configure);
        }
    }
}