using Rabbit;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeadersExample
{
    class Program
    {
        static void Main(string[] args)
        {
            SetupEnviroment();
            var publisher = new Publisher("Publisher-1", ConsoleColor.White);
            publisher.WhenPublish = (sender, channel) =>
            {
                for (int i = 1; i <= 15; i++)
                {
                    string customerType = i % 2 == 0 ? "b2b" : "b2c";
                    string material = i % 2 == 0 ? "wood" : "metal";
                    if (i % 5 == 0) customerType = "unknown";
                    if (i > 10) customerType = "b2b";
                    if (i > 14)
                    {
                        customerType = "b2c";
                        material = "plastic";
                    }


                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.Headers = new Dictionary<string, object>();
                    properties.Headers.Add("customerType", customerType);
                    properties.Headers.Add("material", material);

                    string message = $"Message {i} ({customerType}-{material})";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "HeadersExample.Exchange",
                                         routingKey: "",
                                         basicProperties: properties,
                                         body: body);
                    WriteLine(sender, $"[{sender.Name}] Sent: {message}");
                    Thread.Sleep(1000);
                }
            };

            var consumer1 = new Consumer("Consumer-1-metal or b2c", "HeadersExample.Queue1", ConsoleColor.Green);
            var consumer2 = new Consumer("Consumer-2-wood and b2b", "HeadersExample.Queue2", ConsoleColor.Cyan);
            var consumer3 = new Consumer("Consumer-3-b2c", "HeadersExample.Queue3", ConsoleColor.Blue);

            consumer1.Start();
            consumer2.Start();
            consumer3.Start();
            publisher.Start();
            Console.WriteLine(" Press [enter] to exit.");
        }

        private static void SetupEnviroment()
        {
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            using (var connection = factory.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare("HeadersExample.Exchange", ExchangeType.Headers, true);

                model.QueueDeclare("HeadersExample.Queue1", true, false, false, null);
                model.QueueBind("HeadersExample.Queue1", "HeadersExample.Exchange", "", new Dictionary<string, object> { { "x-match", "any" }, { "material", "metal" }, { "customerType", "b2c" } });

                model.QueueDeclare("HeadersExample.Queue2", true, false, false, null);
                model.QueueBind("HeadersExample.Queue2", "HeadersExample.Exchange", "", new Dictionary<string, object> { { "x-match", "all" }, { "material", "wood" }, { "customerType", "b2b" } });

                model.QueueDeclare("HeadersExample.Queue3", true, false, false, null);
                model.QueueBind("HeadersExample.Queue3", "HeadersExample.Exchange", "", new Dictionary<string, object> { { "x-match", "any" }, { "customerType", "b2c" } });


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
