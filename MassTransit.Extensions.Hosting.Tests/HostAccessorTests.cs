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

using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MassTransit.Extensions.Hosting.Tests
{
    /// <summary />
    public class HostAccessorTests
    {
        /// <summary />
        [Fact]
        public void GetHost_WhenDuplicateName_GivenValid_ThenFirst()
        {
            var mockHost1 = new Mock<IHost>(MockBehavior.Strict);
            var mockHost2 = new Mock<IHost>(MockBehavior.Strict);

            var services = new ServiceCollection();
            services.AddTransient<HostAccessor>();

            services.AddSingleton<IHostAccessor<IHost>>(new HostAccessor<IHost>
            {
                ConnectionName = "test",
                Host = mockHost1.Object
            });

            services.AddSingleton<IHostAccessor<IHost>>(new HostAccessor<IHost>
            {
                ConnectionName = "test",
                Host = mockHost2.Object
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var hostAccessor = serviceProvider.GetRequiredService<HostAccessor>();

                var host1 = hostAccessor.GetHost<IHost>("test");
                Assert.Same(mockHost1.Object, host1);
            }
        }

        /// <summary />
        [Fact]
        public void GetHost_WhenMultiple_GivenValid_ThenSuccess()
        {
            var mockHost1 = new Mock<IHost>(MockBehavior.Strict);
            var mockHost2 = new Mock<IHost>(MockBehavior.Strict);

            var services = new ServiceCollection();
            services.AddTransient<HostAccessor>();

            services.AddSingleton<IHostAccessor<IHost>>(new HostAccessor<IHost>
            {
                ConnectionName = "test1",
                Host = mockHost1.Object
            });

            services.AddSingleton<IHostAccessor<IHost>>(new HostAccessor<IHost>
            {
                ConnectionName = "test2",
                Host = mockHost2.Object
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var hostAccessor = serviceProvider.GetRequiredService<HostAccessor>();

                var host1 = hostAccessor.GetHost<IHost>("test1");
                Assert.Same(mockHost1.Object, host1);

                var host2 = hostAccessor.GetHost<IHost>("test2");
                Assert.Same(mockHost2.Object, host2);
            }
        }

        /// <summary />
        [Fact]
        public void GetHost_WhenSingle_GivenInvalid_ThenNull()
        {
            var mockHost = new Mock<IHost>(MockBehavior.Strict);

            var services = new ServiceCollection();
            services.AddTransient<HostAccessor>();
            services.AddSingleton<IHostAccessor<IHost>>(new HostAccessor<IHost>
            {
                ConnectionName = "test",
                Host = mockHost.Object
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var hostAccessor = serviceProvider.GetRequiredService<HostAccessor>();

                var host = hostAccessor.GetHost<IHost>("other");
                Assert.Null(host);
            }
        }

        /// <summary />
        [Fact]
        public void GetHost_WhenSingle_GivenValid_ThenSuccess()
        {
            var mockHost = new Mock<IHost>(MockBehavior.Strict);

            var services = new ServiceCollection();
            services.AddTransient<HostAccessor>();
            services.AddSingleton<IHostAccessor<IHost>>(new HostAccessor<IHost>
            {
                ConnectionName = "test",
                Host = mockHost.Object
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var hostAccessor = serviceProvider.GetRequiredService<HostAccessor>();

                var host = hostAccessor.GetHost<IHost>("test");
                Assert.Same(mockHost.Object, host);
            }
        }
    }
}