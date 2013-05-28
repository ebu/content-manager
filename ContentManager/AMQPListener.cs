using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ContentManager
{
    class AMQPListener
    {
        private const string EXCHANGE_NAME = "ebu.datagateway";


        IConnection connection = null;
        QueueingBasicConsumer consumer = null;
        IModel channel = null;

        public void start()
        {

            ConnectionFactory factory = new ConnectionFactory();
            factory.HostName = "10.50.213.168";
            factory.Port = 5672;
            factory.UserName = "admin";
            factory.Password = "1234";
            connection = factory.CreateConnection();

            channel = connection.CreateModel();

            String queueName = channel.QueueDeclare().QueueName;

            channel.ExchangeDeclare(EXCHANGE_NAME, "topic");
            channel.QueueBind(queueName, EXCHANGE_NAME, "swisstiming.data");
            channel.QueueBind(queueName, EXCHANGE_NAME, "swisstiming.trace");

            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);

            Console.WriteLine("Successfuly started");

        }

        public BasicDeliverEventArgs getNextMessage()
        {
            BasicDeliverEventArgs e = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
            
            Console.WriteLine(" [x] Received message : " + e.Body);

            return e;
        }

        public void stop()
        {
            if (connection != null)
            {

                channel.Close();
                connection.Close();

            }
        }

    }
}
