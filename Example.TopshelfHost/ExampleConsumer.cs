using System.Threading.Tasks;
using MassTransit;

namespace Example.TopshelfHost
{
    public class ExampleConsumer : IConsumer<IExampleMessage>
    {
        public async Task Consume(ConsumeContext<IExampleMessage> context)
        {
            // do something...
            await Task.CompletedTask.ConfigureAwait(false);
        }

    }
}