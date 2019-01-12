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
using MassTransit;
using MassTransit.Extensions.Hosting;

namespace Example.ConsoleHost
{
    public interface IWidgetService
    {
        void DoSomething();
    }

    public class WidgetService : IWidgetService
    {
        private readonly IBusManager _busManager;

        public WidgetService(IBusManager busManager)
        {
            _busManager = busManager ?? throw new ArgumentNullException(nameof(busManager));
        }

        public virtual void DoSomething()
        {
            var bus = _busManager.GetBus("connection-name-1");

            bus.Publish<IExampleMessage>(new
            {
                CorrelationId = NewId.NextGuid(),
                StringData = "hello world",
                DateTimeData = DateTime.Now,
            });
        }
    }
}