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

namespace ContentManager
{
    public class AMQPEngine
    {



        public delegate void Update(Dictionary<string, string> message);
        public event Update onUpdate;


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

                if (amqpMessage.RoutingKey.StartsWith("trace."))
                {
                    if (onTrace != null) onTrace(message);
                }
                else
                {
                    if (onUpdate != null) onUpdate(message);
                }

            }
        }

    }


}
