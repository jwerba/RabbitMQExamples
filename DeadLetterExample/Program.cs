using Rabbit;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeadLetterExample
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
                    var properties = channel.CreateBasicProperties();

                    string message = $"Message {i}";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "",
                                         routingKey: "DeadLetterExample.Queue1",
                                         basicProperties: null,
                                         body: body);
                    WriteLine(sender, $"[{sender.Name}] Sent: {message}");
                    Thread.Sleep(1000);
                }
            };

            var consumer1 = new Consumer("Consumer-1", "DeadLetterExample.Queue1", ConsoleColor.Green, false);
            consumer1.OnReceive = (sender, eventBasicConsumer, ev) =>
             {
                 var body = ev.Body;
                 var message = Encoding.UTF8.GetString(body);
                 try
                 {
                     int n = int.Parse(message.Replace("Message ", "").Trim());
                     if (n == 13) throw new Exception("Fail on 13");
                     if (n % 4 == 0) //fail 4,8,12...
                     {
                         WriteLine(sender, $" [{sender.Name}] rejected {message}");
                         eventBasicConsumer.Model.BasicReject(ev.DeliveryTag, false);
                     }
                     else
                     {
                         //confirm
                         WriteLine(sender, $" [{sender.Name}] Received {message}");
                         eventBasicConsumer.Model.BasicAck(ev.DeliveryTag, false);
                     }

                 }
                 catch (Exception ex)
                 {
                     WriteLine(sender, $" [{sender.Name}] failed with exception '{ex.Message}' on message {message}");
                     Thread.Sleep(5000);
                 }
             };


            //var consumer2 = new Consumer("Consumer-2", "HeadersExample.Queue2", ConsoleColor.Cyan);
            var consumer2 = new Consumer("Consumer-Errors", "DeadLetterExample.ErrorsQueue", ConsoleColor.Red, false);
            consumer2.OnReceive = (sender, eventBasicConsumer, ev) =>
            {

                var body = ev.Body;
                var message = Encoding.UTF8.GetString(body);
                try
                {

                    if (ev.BasicProperties.Headers.ContainsKey("retry-count"))
                    {
                        int retryCount = (int)ev.BasicProperties.Headers["retry-count"];
                        if (retryCount >= 2)
                        {
                            WriteLine(sender, $" [{sender.Name}] REJECTED DEFINITELY {message}");
                            eventBasicConsumer.Model.BasicReject(ev.DeliveryTag, false);
                            return;
                        }
                    }



                    var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        var basicProperties = channel.CreateBasicProperties();
                        if (ev.BasicProperties.IsMessageIdPresent()) basicProperties.MessageId = ev.BasicProperties.MessageId;

                        basicProperties.Persistent = ev.BasicProperties.Persistent;
                        if (ev.BasicProperties.IsTimestampPresent()) basicProperties.Timestamp = ev.BasicProperties.Timestamp;
                        if (ev.BasicProperties.IsExpirationPresent()) basicProperties.Expiration = ev.BasicProperties.Expiration;
                        if (ev.BasicProperties.IsDeliveryModePresent()) basicProperties.DeliveryMode = ev.BasicProperties.DeliveryMode;
                        if (ev.BasicProperties.IsContentTypePresent()) basicProperties.ContentType = ev.BasicProperties.ContentType;
                        if (ev.BasicProperties.IsContentEncodingPresent()) basicProperties.ContentEncoding = ev.BasicProperties.ContentEncoding;
                        if (ev.BasicProperties.IsClusterIdPresent()) basicProperties.ClusterId = ev.BasicProperties.ClusterId;
                        basicProperties.Headers = new Dictionary<string, object>();// ev.BasicProperties.Headers);
                        int i = 0;
                        if (ev.BasicProperties.Headers.ContainsKey("retry-count"))
                            i = (int)ev.BasicProperties.Headers["retry-count"];
                        i = i + 1;
                        basicProperties.Headers.Add("retry-count", i);
                        var newBody = new byte[ev.Body.Length];
                        ev.Body.CopyTo(newBody, 0);
                        channel.BasicPublish("", "DeadLetterExample.Queue1", basicProperties, newBody);
                        eventBasicConsumer.Model.BasicAck(ev.DeliveryTag, false);
                        WriteLine(sender, $" [{sender.Name}] REPUBLISHED {message} TO QUEUE1");
                    }

                }
                catch (Exception ex)
                {
                    WriteLine(sender, $" [{sender.Name}] failed with exception '{ex.Message}' on message {message}");
                    Thread.Sleep(5000);
                }
            };


            consumer1.Start();
            consumer2.Start();
            publisher.Start();
            Console.WriteLine(" Press [enter] to exit.");
        }

        private static void SetupEnviroment()
        {
            var factory = new ConnectionFactory();
            using (var connection = factory.CreateConnection())
            using (var model = connection.CreateModel())
            {

                model.ExchangeDeclare("DeadLetterExample.ErrorsExchange", ExchangeType.Fanout);
                model.QueueDeclare("DeadLetterExample.ErrorsQueue", true, false, false, new Dictionary<string, object> { { "x-dead-letter-exchange", "DeadLetterExample.DeadLetterExchange" }, { "x-message-ttl", 4000 } });
                model.QueueBind("DeadLetterExample.ErrorsQueue", "DeadLetterExample.ErrorsExchange", "");

                model.ExchangeDeclare("DeadLetterExample.DeadLetterExchange", ExchangeType.Fanout);
                model.QueueDeclare("DeadLetterExample.DeadLettersQueue", true, false, false);
                model.QueueBind("DeadLetterExample.DeadLettersQueue", "DeadLetterExample.DeadLetterExchange", "");

                model.QueueDeclare("DeadLetterExample.Queue1", true, false, false, new Dictionary<string, object> { { "x-dead-letter-exchange", "DeadLetterExample.ErrorsExchange" }, { "x-message-ttl", 2000 } });
            }
        }

        private static void WriteLine(Actor sender, string text)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = sender.Color;
            Console.WriteLine(text);
            Console.ForegroundColor = prev;
        }
    }
}
