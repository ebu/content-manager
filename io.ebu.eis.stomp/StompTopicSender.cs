using System;
using System.Threading;
using Apache.NMS;
using Apache.NMS.Util;

namespace io.ebu.eis.stomp
{
    internal class StompParameters
    {
        public string Uri { get; set; }
        public string Topic { get; set; }
        public string Showparam { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class StompTopicSender
    {

        public void SendStompImage(string uri, string username, string password, string topic, string url, string text, string link)
        {
            if (!topic.EndsWith("/"))
                topic = topic + "/";
            
            var newThread = new Thread(SendToStompThread);
            var param = new StompParameters()
            {
                Uri = uri,
                Topic = topic + "image",
                Text = text,
                Link = link,
                Showparam = "SHOW " + url,
                Username = username,
                Password = password
            };
            newThread.Start(param);

            if (!string.IsNullOrEmpty(text))
            {
                // Send Text Message as well if any text
                var newThread2 = new Thread(SendToStompThread);
                var param2 = new StompParameters()
                {
                    Uri = uri,
                    Topic = topic + "text",
                    Text = text,
                    Link = link,
                    Showparam = "TEXT " + text,
                    Username = username,
                    Password = password
                };
                newThread2.Start(param2);
            }
        }

        private void SendToStompThread(object s)
        {
            StompParameters stomp = (StompParameters)s;
            sendToStomp(stomp.Uri, stomp.Topic, stomp.Text, stomp.Link, stomp.Showparam, stomp.Username, stomp.Password);
        }

        private void sendToStomp(string uri, string topic, string text, string link, string showparam, string username, string password)
        {
            try
            {

                Uri connecturi = new Uri(uri);

                // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.
                IConnectionFactory factory = new NMSConnectionFactory(connecturi);

                using (IConnection connection = factory.CreateConnection(username, password))
                using (ISession session = connection.CreateSession())
                {
                    var destination = SessionUtil.GetDestination(session, topic.Replace("/topic/", ""),
                        DestinationType.Topic);
                    var topicListener = topic;

                    var destinationListener = SessionUtil.GetDestination(session, topicListener.Replace("/topic/", ""),
                        DestinationType.Topic);

                    // Create a consumer and producer 
                    using (IMessageConsumer consumer = session.CreateConsumer(destinationListener))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        // Start the connection so that messages will be processed.
                        connection.Start();

                        producer.RequestTimeout = _receiveTimeout;
                        consumer.Listener += OnMessage;

                        // Send a message
                        ITextMessage request = session.CreateTextMessage(showparam);

                        if (!string.IsNullOrEmpty(link))
                            request.Properties["link"] = link;

                        request.Properties["trigger-time"] = "NOW";

                        request.NMSPriority = MsgPriority.Highest;
                        request.NMSDeliveryMode = MsgDeliveryMode.NonPersistent;
                        request.NMSTimeToLive = TimeSpan.FromSeconds(15);
                        request.NMSDestination = destination;
                        request.Text = showparam;
                        producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                        producer.Send(request);


                        // Wait for the message
                        Semaphore.WaitOne((int) _receiveTimeout.TotalMilliseconds, true);
                        if (_message == null)
                        {
                            // TODO Log no message received
                        }
                        else
                        {
                            // TODO Log message received
                        }
                        producer.Close();
                        consumer.Close();
                    }
                    session.Close();
                    connection.Close();
                }
            }
            catch (TypeLoadException)
            {
                // TODO could mean password is wrong and dll did not load
            }
            catch (Exception)
            {
                // TODO Log Exceptions
                
            }
        }

        private static readonly AutoResetEvent Semaphore = new AutoResetEvent(false);
        private static ITextMessage _message;
        private static TimeSpan _receiveTimeout = TimeSpan.FromSeconds(5);
        protected static void OnMessage(IMessage receivedMsg)
        {
            _message = receivedMsg as ITextMessage;
            Semaphore.Set();
        }
    }
}
