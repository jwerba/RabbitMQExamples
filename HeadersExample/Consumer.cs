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
        private string queueName;
        public Consumer(string name, string queueName, ConsoleColor color)
        {
            this.name = name;
            this.Color = color;
            this.queueName = queueName;
        }

        protected override void Run()
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };
            this.connection = factory.CreateConnection();
            this.channel = connection.CreateModel();
            this.channel.QueueDeclare(queue: queueName,
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
            this.channel.BasicConsume(queue: queueName,
                                 noAck: true,
                                 consumer: eventingBasicConsumer);
        }
    }
}
