using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace io.ebu.eis.mq
{
    internal class AMQClient
    {
        internal IModel Channel { get; set; }

        internal event ConsumerCancelledEventHandler ConsumerCancelled;

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
            get { return Channel; }
        }

        protected virtual void OnConsumerCancelled(ConsumerEventArgs args)
        {
            var handler = ConsumerCancelled;
            if (handler != null) handler(this, args);
        }
    }
}
