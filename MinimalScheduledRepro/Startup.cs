using System;
using GreenPipes;
using MassTransit;
using MinimalScheduledRepro;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[assembly: FunctionsStartup(typeof(Startup))]
namespace MinimalScheduledRepro
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddLogging(cfg =>
                {
                    cfg.AddSerilog(new LoggerConfiguration()
                        .WriteTo.Console()
                        .Enrich.FromLogContext()
                        .CreateLogger());
                })
                .AddMassTransitForAzureFunctions(x =>
                {
                    x.AddConsumer<TestSpecificConsumer>(c =>
                    {
                        c.UseScheduledRedelivery(r => r.Intervals(
                            TimeSpan.FromMinutes(1),
                            TimeSpan.FromMinutes(3)));
                        c.UseMessageRetry(r => r.Immediate(2));
                    });
                    
                    x.AddConsumersFromNamespaceContaining<Startup>();
                }, (context, cfg) =>
                {
                    cfg.UseScheduledRedelivery(r => r.Intervals(
                        TimeSpan.FromMinutes(1),
                        TimeSpan.FromMinutes(3))
                    );
                    cfg.UseMessageRetry(r => r.Immediate(3));
                });
        }
    }
}