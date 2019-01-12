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

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides an abstraction to configure and initialize Bus instances.
    /// </summary>
    public interface IMassTransitBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> Dependency Injection container.
        /// </summary>
        IServiceCollection Services { get; }
    }

    /// <summary>
    /// Provides an implementation of <see cref="IMassTransitBuilder"/>.
    /// </summary>
    public class MassTransitBuilder : IMassTransitBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        public MassTransitBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }
    }
}