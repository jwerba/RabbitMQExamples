using Rabbit;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoutingExample
{
    class Program
    {
        static void Main(string[] args)
        {
            SetupEnviroment();
            var publisher = new Publisher("Publisher-1", ConsoleColor.Blue);

            publisher.WhenPublish = (sender, channel) =>
            {
                //channel.QueueDeclare(queue: "hello", durable: true, exclusive: false, autoDelete: false, arguments: null);                
                for (int i = 1; i <= 15; i++)
                {
                    string routingKey = i % 2 == 0 ? "1" : "2";
                    if (i > 10) routingKey = "3";
                    string message = $"Message {i}";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "RoutingExample.SampleExchange",
                                         routingKey: routingKey,
                                         basicProperties: null,
                                         body: body);
                    WriteLine(sender, $"[{sender.Name}] Sent: {message}");
                    Thread.Sleep(1000);
                }

            };

            var consumer1 = new Consumer("Consumer-1", "RoutingExample.Queue1", ConsoleColor.Green);
            var consumer2 = new Consumer("Consumer-2", "RoutingExample.Queue2", ConsoleColor.Cyan);

            consumer1.Start();
            consumer2.Start();

            publisher.Start();

            Console.WriteLine(" Press [enter] to exit.");
        }

        private static void WriteLine(Publisher sender, string text)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = sender.Color;
            Console.WriteLine(text);
            Console.ForegroundColor = prev;
        }

        private static void SetupEnviroment()
        {
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            using (var connection = factory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.ExchangeDeclare("RoutingExample.SampleExchange", ExchangeType.Direct, true);
                    model.QueueDeclare("RoutingExample.Queue1", true, false, false, null);
                    model.QueueBind("RoutingExample.Queue1", "RoutingExample.SampleExchange", "1");
                    model.QueueDeclare("RoutingExample.Queue2", true, false, false, null);
                    model.QueueBind("RoutingExample.Queue2", "RoutingExample.SampleExchange", "2");
                }
            }

        }
    }
}
