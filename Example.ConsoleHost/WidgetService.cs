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