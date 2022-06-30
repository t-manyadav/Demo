using System;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace inputFunc
{
    public class rabbitMQ_func
    {
        [FunctionName("rabbitMQ_func")]
        public void Run([RabbitMQTrigger("hello", ConnectionStringSetting = "ConnectionStringSetting")]BasicDeliverEventArgs args, 
        [RabbitMQ(QueueName = "outputQueue", ConnectionStringSetting = "ConnectionStringSetting" )]out string outputMessage,
        ILogger log)
        {
            log.LogInformation($"C# RabbitMQ queue trigger function processed message: {Encoding.UTF8.GetString(args.Body.ToArray())}");
            log.LogInformation($"C# Message has correlation-id: {(args.BasicProperties.CorrelationId)}");
            outputMessage=Encoding.UTF8.GetString(args.Body.ToArray());
        }
    }
}
