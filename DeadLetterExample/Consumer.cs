using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit
{

    public delegate void OnReceiveDelgate(Consumer sender, EventingBasicConsumer eventBasicConsumer, BasicDeliverEventArgs args);

    public class Consumer : Actor
    {
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer eventingBasicConsumer;
        private string queueName;
        private bool autoAck;
        public Consumer(string name, string queueName, ConsoleColor color, bool autoAck = true)
        {
            this.Name = name;
            this.Color = color;
            this.queueName = queueName;
            this.autoAck = autoAck;
        }

        public OnReceiveDelgate OnReceive = null;


        protected override void Run()
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };
            factory.AutomaticRecoveryEnabled = true;
            this.connection = factory.CreateConnection();
            this.channel = connection.CreateModel();
            this.eventingBasicConsumer = new EventingBasicConsumer(channel);
            this.eventingBasicConsumer.Received += (consumer, args) =>
            {
                if (OnReceive != null)
                    OnReceive(this, (EventingBasicConsumer)consumer, args);
            };
            this.channel.BasicConsume(queue: queueName,
                                 noAck: this.autoAck, //this is key for the server to expect the ACK to mark the msg as delivered and done
                                 consumer: eventingBasicConsumer);
        }
    }
}
