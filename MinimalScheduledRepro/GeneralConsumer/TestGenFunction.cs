using System.Threading;
using System.Threading.Tasks;
using MassTransit.WebJobs.ServiceBusIntegration;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MinimalScheduledRepro
{
    public class TestGenFunction
    {
        private readonly IMessageReceiver _receiver;

        public TestGenFunction(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }
        
        [FunctionName("TestGenFunction")]
        public async Task RunAsync([ServiceBusTrigger("generic", Connection = "")]
            Message message, ILogger log, CancellationToken cancellationToken)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message");
            
            await _receiver.HandleConsumer<TestGenConsumer>("generic", message, cancellationToken);
        }
    }
}