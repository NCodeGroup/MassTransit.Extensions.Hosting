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
    /// Provides a factory method to configure and create bus instances.
    /// </summary>
    public interface IBusFactory<out TConfigurator>
        where TConfigurator : class, IBusFactoryConfigurator
    {
        /// <summary>
        /// Configure and create a bus instance.
        /// </summary>
        /// <param name="configure">The configuration callback to configure the bus.</param>
        /// <returns><see cref="IBusControl"/></returns>
        IBusControl Create(Action<TConfigurator> configure);
    }
}