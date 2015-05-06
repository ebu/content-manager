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
    public class AMQConsumer : INotifyPropertyChanged, IDisposable
    {
        private readonly string _amqpUri;
        private readonly string _amqpExchange;

        private readonly IDataMessageHandler _handler;

        private Thread _t;
        private bool _running;

        private AMQClient _amq;
        private IConnection _conn;
        private QueueingBasicConsumer _consumer;

        private bool _connected;
        public bool Connected { get { return _connected; } set { _connected = value; OnPropertyChanged("Connected"); } }
        private string _filter;

        public AMQConsumer(string uri, string exchange, IDataMessageHandler handler)
        {
            _amqpUri = uri;
            _amqpExchange = exchange;
            _handler = handler;
        }

        public void Connect(string filter = "#")
        {
            try
            {
                _filter = filter;
                var factory = new ConnectionFactory {Uri = _amqpUri};

                _conn = factory.CreateConnection();
                _amq = new AMQClient {Channel = _conn.CreateModel()};

                _amq.Channel.ExchangeDeclare(_amqpExchange, "topic");
                var queueName = _amq.Channel.QueueDeclare();

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
                _amq.Channel.QueueBind(queueName, _amqpExchange, filter);
                //}

                //Console.WriteLine(" [*] Waiting for messages. " + "To exit press CTRL+C");

                _consumer = new QueueingBasicConsumer(_amq.Channel);
                _amq.Channel.BasicConsume(queueName, true, _consumer);

                // Start Processing in other Thread
                _running = true;
                _t = new Thread(Process);
                _t.Start();

                Console.WriteLine("AMQConsumer started and connected to " + _amqpUri + ":" + _amqpExchange);
                Connected = true;
            }
            catch (BrokerUnreachableException)
            {
                // Retry in 1sec
                Thread.Sleep(1000);
                Connect(_filter);
            }
        }

        public void Disconnect()
        {
            _running = false;
            if (_t != null && _t.IsAlive)
                _t.Abort();
            try
            {
                _consumer.Queue.Enqueue(new BasicDeliverEventArgs());
                _consumer.Queue.Close();
                _amq.Channel.Close();
                _conn.Close();
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
                    //var routingKey = ea.RoutingKey;

                    try
                    {
                        var data = DataMessage.Deserialize(message);
                        _handler.OnReceive(data);
                    }
                    catch (Exception)
                    {
                        // TODO Handle exceptions
                    }

                    // TODO use notifications
                    //Console.WriteLine(" [x] Received '{0}': with key :'{1}'", routingKey, text.Key);
                }
                catch (EndOfStreamException)
                {
                    // Try to reconnect
                    Disconnect();
                    Connect(_filter);
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
