using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using CsvHelper.Configuration;
using CsvHelper;
using ContentManager.GUI.Modules.Sports.SwisstimingData;
using System.IO;
using System.Threading;
using log4net;

namespace ContentManager
{
    public class AMQPEngine
    {


        private static readonly ILog log = LogManager.GetLogger(typeof(AMQPEngine));

        public delegate void Data(Dictionary<string, string> message);
        public event Data onData;


        public delegate void Trace(Dictionary<string, string> message);
        public event Trace onTrace;

        private AMQPListener amqpListener = new AMQPListener();

        public void start()
        {
            amqpListener.start();


            Thread newMessageThread;
            newMessageThread = new Thread(new ThreadStart(NewMessageLoop));
            newMessageThread.Start();

        }


        public void NewMessageLoop()
        {

            while (Thread.CurrentThread.IsAlive)
            {

                BasicDeliverEventArgs amqpMessage = amqpListener.getNextMessage();
                string messageBody = System.Text.Encoding.UTF8.GetString(amqpMessage.Body);
                Dictionary<string, string> message = JsonConvert.DeserializeObject<Dictionary<string, string>>(messageBody);

                if (amqpMessage.RoutingKey.EndsWith("trace"))
                {
                    if (onTrace != null) onTrace(message);
                }
                else if (amqpMessage.RoutingKey.EndsWith("data"))
                {
                    if (onData != null) onData(message);
                }
                else
                {
                    log.Warn("Unknown message with routing key : "+ amqpMessage.RoutingKey +" " + messageBody);
                }

            }
        }

    }


}
