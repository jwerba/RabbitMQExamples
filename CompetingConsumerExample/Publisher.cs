using RabbitMQ.Client;
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
                for (int i = 1; i <= 10; i++)
                {
                    
                    string message = $"Message {i}";
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "hello",
                                         basicProperties: null,
                                         body: body);
                    base.WriteLine($"[{name}] Sent: {message}");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
