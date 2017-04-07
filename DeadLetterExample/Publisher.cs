using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit
{
    public delegate void PublishDelegate(Publisher sender, IModel model);
    public class Publisher : Actor
    {

        public Publisher(string name, ConsoleColor color)
        {
            this.Color = color;
            this.Name = name;
        }

        public PublishDelegate WhenPublish = null;
        

        protected override void Run()
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                if (WhenPublish != null)
                    WhenPublish(this, channel);
                }
        }
    }
}
