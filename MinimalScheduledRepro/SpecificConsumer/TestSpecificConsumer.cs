using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace MinimalScheduledRepro
{
    public class TestSpecificConsumer : IConsumer<IMessage>
    {
        private readonly ILogger<TestSpecificConsumer> _logger;

        public TestSpecificConsumer(ILogger<TestSpecificConsumer> logger)
        {
            _logger = logger;
        }
        
        public Task Consume(ConsumeContext<IMessage> context)
        {
            _logger.LogInformation("Executing specfic");
            
            throw new Exception("Testing failure");
        }
    }
}