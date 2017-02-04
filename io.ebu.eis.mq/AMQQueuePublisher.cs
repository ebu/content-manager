using System;
using System.Text;
using System.Threading;
using io.ebu.eis.datastructures;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace io.ebu.eis.mq
{
    public class AMQQueuePublisher : IDisposable
    {
        private readonly string _amqpUri;
        private readonly string _amqpExchange;

        private ConnectionFactory _factory;
        private IConnection _conn;
        private AMQClient _amq;

        public bool Connected { get; private set; }

        public AMQQueuePublisher(string uri, string exchange)
        {
            _amqpUri = uri;
            _amqpExchange = exchange;
        }

        public void Connect(string filter = "#")
        {
            try
            {
                _factory = new ConnectionFactory { Uri = _amqpUri };

                _conn = _factory.CreateConnection();
                _amq = new AMQClient { Channel = _conn.CreateModel() };
                _amq.Channel.QueueDeclare(_amqpExchange, true, false, false, null);

                Console.WriteLine("AMQPublisher started and connected to queue " + _amqpUri + ":" + _amqpExchange);
                Connected = true;
            }
            catch (BrokerUnreachableException)
            {
                // TODO Log
            }
        }

        public void ConnectAsync(string filter = "#")
        {
            var t = new Thread(() => Connect(filter));
            t.Start();
        }


        public void Disconnect()
        {
            if (_amq != null)
                _amq.Channel.Close();
            if (_conn != null && _conn.IsOpen)
                _conn.Close();
        }

        public void Dispatch(string message)
        {
            // Create Persistence
            if (_amq != null)
            {
                var properties = _amq.Channel.CreateBasicProperties();
                properties.SetPersistent(true);

                _amq.Channel.BasicPublish("", _amqpExchange, properties, Encoding.UTF8.GetBytes(message));
            }
            else
            {
                // TODO LOG 
            }
        }
        public void Dispatch(DispatchNotificationMessage message)
        {
            Dispatch(message.Serialize());
        }


        public void Dispose()
        {
            if (Connected)
                Disconnect();
        }
    }
}
