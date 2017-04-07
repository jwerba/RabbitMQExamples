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

        public Consumer(string name, ConsoleColor color)
        {
            this.name = name;
            base.color = color;
        }

        protected override void Run()
        {

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

                var body = args.Body;
                var message = Encoding.UTF8.GetString(body);
                WriteLine($" [{name}] Received {message}");
            };
            this.channel.BasicConsume(queue: "hello",
                                 noAck: true,
                                 consumer: eventingBasicConsumer);
        }
    }
}
