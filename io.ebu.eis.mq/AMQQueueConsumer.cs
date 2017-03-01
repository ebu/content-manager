using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using io.ebu.eis.datastructures;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace io.ebu.eis.mq
{
    public class AMQQueueConsumer : INotifyPropertyChanged, IDisposable
    {
        private readonly string _amqpUri;
        private readonly string _amqpQueue;

        private readonly IDataMessageHandler _handler;

        private Thread _t;
        private bool _cancelled;
        private bool _running;

        private IModel _channel;
        private IConnection _connection;
        private QueueingBasicConsumer _consumer;

        private bool _connected;
        public bool Connected { get { return _connected; } set { _connected = value; OnPropertyChanged("Connected"); } }

        public AMQQueueConsumer(string uri, string queue, IDataMessageHandler handler)
        {
            _amqpUri = uri;
            _amqpQueue = queue;
            _handler = handler;
        }

        public void Connect()
        {
            try
            {
                var factory = new ConnectionFactory { Uri = _amqpUri };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(queue: _amqpQueue,
                    durable: true, exclusive: false, autoDelete: false, arguments: null);

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);


                Console.WriteLine(" [*] Waiting for messages.");
                _consumer = new QueueingBasicConsumer(_channel);
                
                _channel.BasicConsume(queue: _amqpQueue, noAck: false, consumer: _consumer);

                // Start Processing in other Thread
                _running = true;
                _t = new Thread(Process);
                _t.Start();

                Console.WriteLine("AMQQueueConsumer started and connected to " + _amqpUri + ":" + _amqpQueue);
                Connected = true;
            }
            catch (BrokerUnreachableException)
            {
                // Retry in 1sec
                if (!_cancelled)
                {
                    Thread.Sleep(1000);
                    Connect();
                }
            }
        }

        public void ConnectAsync()
        {
            var t = new Thread(() => Connect());
            t.Start();
        }

        public void Disconnect()
        {
            _cancelled = true;
            _running = false;
            if (_t != null && _t.IsAlive)
                _t.Abort();
            try
            {
                _consumer.Queue.Enqueue(new BasicDeliverEventArgs());
                _consumer.Queue.Close();
                //_amq.Channel.Close();
                _connection.Close();
                Connected = false;
            }
            catch (Exception)
            {
                // TODO Log
            }
        }

        private void Process()
        {
            while (_running)
            {
                try
                {
                    var ea = _consumer.Queue.Dequeue();
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    Console.WriteLine(" [x] Received {0}", message.Substring(1, 100));


                    var handled = false;
                    try
                    {
                        handled = _handler.HandleWorkerTask(message);
                    }
                    catch (Exception)
                    {
                        // TODO Handle exceptions
                    }

                    if (handled)
                    {
                        Console.WriteLine(" [x] Done");
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        Console.WriteLine(" [x] NACK !");
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }

                }
                catch (EndOfStreamException)
                {
                    //Try to reconnect
                    Disconnect();
                    Connect();
                }
            }
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }
        #endregion PropertyChanged

        public void Dispose()
        {
            Disconnect();
        }
    }
}
