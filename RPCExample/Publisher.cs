using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit
{
    public class Publisher : Actor
    {
        private EventingBasicConsumer eventingBasicConsumer;
        private string name;

        public Publisher(string name, ConsoleColor color)
        {
            base.color = color;
            this.name = name;
        }

        protected override void Run()
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                //Dynamically create a temporal queue (QueueDeclare() without parameters) for the responses 
                string responseQueueName = channel.QueueDeclare().QueueName;
                string correlationID = Guid.NewGuid().ToString();

                //Listen a temp queue for the result response
                this.eventingBasicConsumer = new EventingBasicConsumer(channel);
                this.eventingBasicConsumer.Received += (model, args) =>
                {
                    var body = args.Body;
                    var message = Encoding.UTF8.GetString(body);

                    if (correlationID == args.BasicProperties.CorrelationId)
                    {
                        
                        WriteLine($" [{name}] Received {message}");
                    }
                    else
                    {
                        WriteLine($"WRONG MESSAGE. Invalid correlationID. Message= {message}");
                    }
                };

                channel.BasicConsume(queue: responseQueueName,
                                     noAck: true,
                                     consumer: eventingBasicConsumer);



                //Send logic...

                for (int i = 1; i <= 10; i++)
                {
                    
                    string message = $"{i}";
                    var body = Encoding.UTF8.GetBytes(message);

                    //create basic properties to specify the ReplyTo property having the responseQueueName 
                    //for the consumer to know into which queue to put the response
                    var basicProperties = channel.CreateBasicProperties();
                    basicProperties.ReplyTo = responseQueueName;
                    basicProperties.CorrelationId = correlationID;

                    channel.BasicPublish(exchange: "",
                                         routingKey: "hello",
                                         basicProperties: basicProperties, //set the basicProperties
                                         body: body);
                    base.WriteLine($"[{name}] Sent: {message}");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
