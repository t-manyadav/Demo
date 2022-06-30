using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Azure.Communication;
using Azure.Communication.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Text;

class Send
{
    private static ActivitySource source = new ActivitySource("RabbitMQ.Producer");
    public static void Main()
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MySample"))
                .AddSource("RabbitMQ.Producer")
                .AddConsoleExporter()
                .AddAzureMonitorTraceExporter(o =>
                {
                    o.ConnectionString =
                    "InstrumentationKey=2fd37b69-0dcf-4b74-aaab-37cdf898f367;IngestionEndpoint=https://centralindia-0.in.applicationinsights.azure.com/;LiveEndpoint=https://centralindia.livediagnostics.monitor.azure.com/";
                })
                .Build();

        using (Activity activity = source.StartActivity("RabbitMQ.Producer"))
        {
            var factory = new ConnectionFactory() 
            { 
                Uri = new Uri("amqp://user:PASSWORD@20.124.171.130:5672")
            };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                IBasicProperties props= 
                channel.CreateBasicProperties();
                props.ContentType="text/plain";
                props.DeliveryMode=2;
                props.CorrelationId=Activity.Current?.Id;

                channel.BasicPublish(exchange: "",
                                    routingKey: "hello",
                                    basicProperties: props,
                                    body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        
    }
}