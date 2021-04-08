using System;
using GreenPipes;
using MassTransit;
using MassTransit.WebJobs.ServiceBusIntegration;
using MinimalScheduledRepro;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
                .AddSingleton<IMessageReceiver, MessageReceiver>()
                .AddSingleton<IAsyncBusHandle, AsyncBusHandle>()
                .AddMassTransit(x =>
                {
                    x.AddServiceBusMessageScheduler();
                    x.UsingAzureServiceBus((context, cfg) =>
                    {
                        var options = context.GetRequiredService<IOptions<ServiceBusOptions>>();

                        options.Value.MessageHandlerOptions.AutoComplete = true;

                        cfg.Host(options.Value.ConnectionString, h =>
                        {
                            if (IsMissingCredentials(options.Value.ConnectionString))
                                h.TokenProvider = new ManagedIdentityTokenProvider(new AzureServiceTokenProvider());
                        });
                        
                        cfg.UseServiceBusMessageScheduler();
                        cfg.ConfigureEndpoints(context);
                        
                        cfg.UseScheduledRedelivery(r => r.Intervals(
                            TimeSpan.FromMinutes(1),
                            TimeSpan.FromMinutes(3))
                        );
                        cfg.UseMessageRetry(r => r.Immediate(3));
                    });
                    
                    x.AddConsumer<TestSpecificConsumer>(c =>
                    {
                        c.UseScheduledRedelivery(r => r.Intervals(
                            TimeSpan.FromMinutes(1),
                            TimeSpan.FromMinutes(3)));
                        c.UseMessageRetry(r => r.Immediate(2));
                    });
                    
                    x.AddConsumersFromNamespaceContaining<Startup>();
                });
                // .AddMassTransitForAzureFunctions(x =>
                // {
                //     x.AddConsumer<TestSpecificConsumer>(c =>
                //     {
                //         c.UseScheduledRedelivery(r => r.Intervals(
                //             TimeSpan.FromMinutes(1),
                //             TimeSpan.FromMinutes(3)));
                //         c.UseMessageRetry(r => r.Immediate(2));
                //     });
                //     
                //     x.AddConsumersFromNamespaceContaining<Startup>();
                // }, (context, cfg) =>
                // {
                //     cfg.UseScheduledRedelivery(r => r.Intervals(
                //         TimeSpan.FromMinutes(1),
                //         TimeSpan.FromMinutes(3))
                //     );
                //     cfg.UseMessageRetry(r => r.Immediate(3));
                // });
        }
        
        static bool IsMissingCredentials(string connectionString)
        {
            var builder = new ServiceBusConnectionStringBuilder(connectionString);

            return string.IsNullOrWhiteSpace(builder.SasKeyName) && string.IsNullOrWhiteSpace(builder.SasKey) && string.IsNullOrWhiteSpace(builder.SasToken);
        }
    }
}