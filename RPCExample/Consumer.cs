using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit
{
    public class Consumer : Actor
    {
        private string name;
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer eventingBasicConsumer;
        private static Random random = new Random();


        public Consumer(string name, ConsoleColor color)
        {
            this.name = name;
            base.color = color;
        }

        protected override void Run()
        {
            int factor = random.Next(1, 10);
            WriteLine($"FACTOR {factor}");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            this.connection = factory.CreateConnection();
            this.channel = connection.CreateModel();
            this.channel.QueueDeclare(queue: "hello",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            this.eventingBasicConsumer = new EventingBasicConsumer(channel);
            this.eventingBasicConsumer.Received += (model, args) =>
            {
                var message = Encoding.UTF8.GetString(args.Body);
                WriteLine($" [{name}] Received {message}");

                //reply
                //var channel = (IModel)model;
                var basicProperties = channel.CreateBasicProperties();
                basicProperties.CorrelationId = args.BasicProperties.CorrelationId;
                int x = int.Parse(message);
                int result = x * factor;
                var responseBody = Encoding.UTF8.GetBytes($"{factor} x {x} = {result}");
                this.channel.BasicPublish("", args.BasicProperties.ReplyTo, basicProperties, responseBody);
               
            };

            this.channel.BasicConsume(queue: "hello",
                                 noAck: true,
                                 consumer: eventingBasicConsumer);
        }
    }
}
