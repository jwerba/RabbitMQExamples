using Rabbit;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TopicExample
{
    class Program
    {

        static void Main(string[] args)
        {
            SetupEnviroment();
            var publisher = new Publisher("Publisher-1", ConsoleColor.Blue);
            publisher.WhenPublish = (sender, channel) =>
            {
                for (int i = 1; i <= 15; i++)
                {
                    string routingKey = i % 2 == 0 ? "orange.3.high" : "apple.3.low";
                    if (i > 10) routingKey = "corporate.3.high";
                    if (i > 14) routingKey = "grappe.4.medium";
                    string message = $"Message {i} ({routingKey})";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "TopicExample.Exchange",
                                         routingKey: routingKey,
                                         basicProperties: null,
                                         body: body);
                    WriteLine(sender, $"[{sender.Name}] Sent: {message}");
                    Thread.Sleep(1000);
                }
            };

            var consumer1 = new Consumer("Consumer-1", "TopicExample.HighQueue", ConsoleColor.Green);
            var consumer2 = new Consumer("Consumer-2", "TopicExample.LowQueue", ConsoleColor.Cyan);
            var consumer3 = new Consumer("Consumer-3", "TopicExample.CorporateQueue", ConsoleColor.DarkMagenta);

            consumer1.Start();
            consumer2.Start();
            consumer3.Start();
            publisher.Start();
            Console.WriteLine(" Press [enter] to exit.");
        }


        private static void SetupEnviroment()
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            using (var connection = factory.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare("TopicExample.Exchange", ExchangeType.Topic, true);

                model.QueueDeclare("TopicExample.HighQueue", true, false, false, null);
                model.QueueBind("TopicExample.HighQueue", "TopicExample.Exchange", "*.*.high");

                model.QueueDeclare("TopicExample.LowQueue", true, false, false, null);
                model.QueueBind("TopicExample.LowQueue", "TopicExample.Exchange", "*.*.low");

                model.QueueDeclare("TopicExample.CorporateQueue", true, false, false, null);
                model.QueueBind("TopicExample.CorporateQueue", "TopicExample.Exchange", "corporate.#");
                model.QueueBind("TopicExample.CorporateQueue", "TopicExample.Exchange", "*.*.medium");
            }
        }


        private static void WriteLine(Publisher sender, string text)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = sender.Color;
            Console.WriteLine(text);
            Console.ForegroundColor = prev;
        }
    }
}
