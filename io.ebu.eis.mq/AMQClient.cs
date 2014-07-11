using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.ebu.eis.mq
{
    internal class AMQClient
    {
        internal IModel channel { get; set; }


        internal event RabbitMQ.Client.Events.ConsumerCancelledEventHandler ConsumerCancelled;

        internal void HandleBasicCancel(string consumerTag)
        {
            Console.WriteLine("CANCEL " + consumerTag);
        }

        internal void HandleBasicCancelOk(string consumerTag)
        {
            Console.WriteLine("CANCEL OK " + consumerTag);
        }

        internal void HandleBasicConsumeOk(string consumerTag)
        {
            Console.WriteLine("CONSUME OK " + consumerTag);
        }

        internal void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            Console.WriteLine("DELIVER " + consumerTag + " / " + deliveryTag + " / " + redelivered + " / " + exchange + " / " + routingKey);
            Console.WriteLine("BODY : " + body);
        }

        internal void HandleModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            // TODO
        }

        internal IModel Model
        {
            get { return channel; }
        }
    }
}
