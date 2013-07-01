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
using Newtonsoft.Json.Linq;

namespace ContentManager
{
    public class AMQPEngine
    {


        private static readonly ILog logger = LogManager.GetLogger(typeof(AMQPEngine));

        public delegate void Event(JObject jsonObject);
        public event Event onEvent;

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
                //Dictionary<string, string> message = JsonConvert.DeserializeAnonymousType<Dictionary<string, >>(messageBody);

                
                var myObjects = JsonConvert.DeserializeObject<JObject>(messageBody);

                logger.Info("New notification of type : " + myObjects["type"]);

                onEvent(myObjects);


            }
        }
    }


}
