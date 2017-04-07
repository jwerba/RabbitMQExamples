using System;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using System.Threading;
using Rabbit;

namespace Rabbit
{
    class Program
    {
        static void Main(string[] args)
        {
            var publisher = new Publisher("Publisher-1", ConsoleColor.Blue);

            var consumer1 = new Consumer("Consumer-1", ConsoleColor.Green);
            var consumer2 = new Consumer("Consumer-2", ConsoleColor.Cyan);

            consumer1.Start();
            consumer2.Start();

            publisher.Start();

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        
    }
}
