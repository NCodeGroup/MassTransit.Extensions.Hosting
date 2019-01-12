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
    /// Provides a factory method to configure and create in-memory bus instances.
    /// </summary>
    public interface IInMemoryBusFactory : IBusFactory<IInMemoryBusFactoryConfigurator>
    {
        /// <summary>
        /// Configure and create an in-memory bus instance.
        /// </summary>
        /// <param name="baseAddress">Override the default base address.</param>
        /// <param name="configure">The configuration callback to configure the bus.</param>
        /// <returns><see cref="IBusControl"/></returns>
        IBusControl Create(Uri baseAddress, Action<IInMemoryBusFactoryConfigurator> configure);
    }

    /// <summary>
    /// Provides an implementation of <see cref="IBusFactory{TConfigurator}"/>
    /// that creates in-memory bus instances using <see cref="IInMemoryBusFactoryConfigurator"/>.
    /// </summary>
    public class InMemoryBusFactory : IInMemoryBusFactory
    {
        /// <inheritdoc />
        public virtual IBusControl Create(Action<IInMemoryBusFactoryConfigurator> configure)
        {
            return Bus.Factory.CreateUsingInMemory(configure);
        }

        /// <inheritdoc />
        public virtual IBusControl Create(Uri baseAddress, Action<IInMemoryBusFactoryConfigurator> configure)
        {
            return Bus.Factory.CreateUsingInMemory(baseAddress, configure);
        }

    }
}