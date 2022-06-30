using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Azure.Communication;
using Azure.Communication.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using OpenTelemetry;
using System.Diagnostics;
using OpenTelemetry.Exporter;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(inputFunc.Startup))]
namespace inputFunc
{
    public class Startup : FunctionsStartup
    {
         private static ActivitySource source = new ActivitySource("Microsoft.Azure.WebJobs.Host");
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceName = "RabbitMQ.Functions";
            var serviceVersion = "1.0.0";

            var openTelemetry = Sdk.CreateTracerProviderBuilder()
                .AddSource("Microsoft.Azure.WebJobs.Host")
                .AddSource("Microsoft.Azure.WebJobs.Extensions.RabbitMQ")
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                //.AddProcessor(new MyFilteringProcessor(
                //    //new SimpleActivityExportProcessor(new ConsoleExporter()),// MyExporter("ExporterX")),
                //    (act) => Validate(act)))
                .AddConsoleExporter()
                // .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"))
                .AddAzureMonitorTraceExporter(o =>
                {
                    o.ConnectionString =
                    "InstrumentationKey=2fd37b69-0dcf-4b74-aaab-37cdf898f367;IngestionEndpoint=https://centralindia-0.in.applicationinsights.azure.com/;LiveEndpoint=https://centralindia.livediagnostics.monitor.azure.com/";
                })
                // .AddZipkinExporter(o => o.HttpClientFactory = () =>
                // {
                //     HttpClient client = new HttpClient();
                //     client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value");
                //     return client;
                // })
                .Build();
                using (Activity activity = source.StartActivity("Microsoft.Azure.WebJobs.Host"))
                {
                    Console.WriteLine("Hey! I'm Startup.cs activity");
                }
            builder.Services.AddSingleton(openTelemetry);
            AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);
        }
    }
}