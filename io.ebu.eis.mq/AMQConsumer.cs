using io.ebu.eis.datastructures;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SMPAG.MM.MMConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace io.ebu.eis.mq
{
    public class AMQConsumer
    {
        private string _amqpUri;
        private string _amqpExchange;

        private IAMQDataMessageHandler _handler;

        private bool _running = false;

        QueueingBasicConsumer _consumer;

        public AMQConsumer(string uri, string exchange, IAMQDataMessageHandler handler)
        {
            _amqpUri = uri;
            _amqpExchange = exchange;
            _handler = handler;
        }

        public void Connect(string filter = "#")
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = _amqpUri;

            var conn = factory.CreateConnection();
            var amq = new AMQClient();
            amq.channel = conn.CreateModel();

            amq.channel.ExchangeDeclare(_amqpExchange, "topic");
            var queueName = amq.channel.QueueDeclare();

            // TODO Handle filtering of message
            //if (args.Length < 1)
            //{
            //    Console.Error.WriteLine("Usage: {0} [binding_key...]",
            //                            Environment.GetCommandLineArgs()[0]);
            //    Environment.ExitCode = 1;
            //    return;
            //}
                        
            //foreach (var bindingKey in args)
            //{
            amq.channel.QueueBind(queueName, _amqpExchange, filter);
            //}

            //Console.WriteLine(" [*] Waiting for messages. " + "To exit press CTRL+C");

            _consumer = new QueueingBasicConsumer(amq.channel);
            amq.channel.BasicConsume(queueName, true, _consumer);

            // Start Processing in other Thread
            _running = true;
            Thread t = new Thread(Process);
            t.Start();

            Console.WriteLine("AMQConsumer started and connected to " + _amqpUri + ":" + _amqpExchange);
        }

        public void Disconnect()
        {
            _running = false;
        }

        private void Process()
        {
            while (_running)
            {
                var ea = (BasicDeliverEventArgs)_consumer.Queue.Dequeue();
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                DataMessage text = new DataMessage();
                try
                {
                    var data = DataMessage.Deserialize(message);
                    _handler.OnReceive(data);
                }
                catch (Exception e)
                {
                    // TODO Handle exceptions
                }

                // TODO use notifications
                //Console.WriteLine(" [x] Received '{0}': with key :'{1}'", routingKey, text.Key);
            }
        }
    }
}
