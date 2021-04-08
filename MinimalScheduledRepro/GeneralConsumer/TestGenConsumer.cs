using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace MinimalScheduledRepro
{
    public class TestGenConsumer : IConsumer<IMessage>
    {
        private readonly ILogger<TestGenConsumer> _logger;

        public TestGenConsumer(ILogger<TestGenConsumer> logger)
        {
            _logger = logger;
        }
        
        public Task Consume(ConsumeContext<IMessage> context)
        {
            _logger.LogInformation("Executing generic");
            
            throw new Exception("Testing failure");
        }
    }
}