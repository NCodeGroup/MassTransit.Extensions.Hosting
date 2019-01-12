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

using System.Linq;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Scoping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace MassTransit.Extensions.Hosting.Tests
{
    /// <summary />
    public class ServiceCollectionExtensionsTests
    {
        /// <summary />
        [Fact]
        public void AddMassTransit_ThenValid()
        {
            var services = new ServiceCollection();

            var configuratorWasCalled = false;
            services.AddMassTransit(builder =>
            {
                configuratorWasCalled = true;
                Assert.Same(builder.Services, services);
            });

            Assert.True(configuratorWasCalled);

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Singleton &&
                item.ServiceType == typeof(IBusManager) &&
                item.ImplementationType == typeof(BusManager));

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Singleton &&
                item.ServiceType == typeof(IHostedService) &&
                item.ImplementationFactory != null);

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Transient &&
                item.ServiceType == typeof(IConsumerScopeProvider) &&
                item.ImplementationType == typeof(DependencyInjectionConsumerScopeProvider));

            Assert.Contains(services, item =>
                item.Lifetime == ServiceLifetime.Transient &&
                item.ServiceType == typeof(IConsumerFactory<>) &&
                item.ImplementationType == typeof(ScopeConsumerFactory<>));
        }

        /// <summary />
        [Fact]
        public void AddMassTransit_WhenExisting_ThenSingle()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IBusManager, BusManager>();
            services.AddSingleton<IHostedService>(serviceProvider => serviceProvider.GetRequiredService<IBusManager>());
            services.AddTransient<IConsumerScopeProvider, DependencyInjectionConsumerScopeProvider>();
            services.Add(ServiceDescriptor.Transient(typeof(IConsumerFactory<>), typeof(ScopeConsumerFactory<>)));

            services.AddMassTransit(builder => { });

            Assert.Equal(1, services.Count(item => item.ServiceType == typeof(IBusManager)));
            Assert.Equal(1, services.Count(item => item.ServiceType == typeof(IHostedService)));
            Assert.Equal(1, services.Count(item => item.ServiceType == typeof(IConsumerScopeProvider)));
            Assert.Equal(1, services.Count(item => item.ServiceType == typeof(IConsumerFactory<>)));
        }
    }
}