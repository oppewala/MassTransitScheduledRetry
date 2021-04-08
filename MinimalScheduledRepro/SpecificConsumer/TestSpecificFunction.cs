using System.Threading;
using System.Threading.Tasks;
using MassTransit.WebJobs.ServiceBusIntegration;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MinimalScheduledRepro
{
    public class TestSpecificFunction
    {
        private readonly IMessageReceiver _receiver;

        public TestSpecificFunction(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }
        
        [FunctionName("TestSpecificFunction")]
        public async Task RunAsync([ServiceBusTrigger("specific", Connection = "")]
            Message message, ILogger log, CancellationToken cancellationToken)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message");

            await _receiver.HandleConsumer<TestSpecificConsumer>("specific", message, cancellationToken);
        }
    }
}