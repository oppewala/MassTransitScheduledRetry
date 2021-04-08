using System;
using System.IO;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MinimalScheduledRepro
{
    public class AddQueue
    {
        private readonly IBus _bus;

        public AddQueue(IBus bus)
        {
            _bus = bus;
        }
        
        [FunctionName("AddQueue")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string queueName = req.Query["queue"];

            var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"queue:{queueName}"));
            await sendEndpoint.Send<IMessage>(new { });

            return new OkResult();
            
        }
    }
}