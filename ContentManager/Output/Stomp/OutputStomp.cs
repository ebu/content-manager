/*
Copyright (C) 2010-2012 European Broadcasting Union
http://www.ebulabs.org
*/
/*
This file is part of ebu-content-manager.
https://code.google.com/p/ebu-content-manager/

EBU-content-manager is free software: you can redistribute it and/or modify
it under the terms of the GNU LESSER GENERAL PUBLIC LICENSE as
published by the Free Software Foundation, version 3.
EBU-content-manager is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU LESSER GENERAL PUBLIC LICENSE for more details.

You should have received a copy of the GNU LESSER GENERAL PUBLIC LICENSE
along with EBU-content-manager.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Apache.NMS;
using Apache.NMS.Util;
using ContentManager.GUI;
using SlideGeneratorLib;
using XMLConfig.CMS;

namespace ContentManager.Output.Stomp
{
    public class OutputStomp
    {

        public delegate void OutputEvent(String filename);
        public event OutputEvent onUploadEnd;
        private SlideGeneratorLib.SlideGenerator slidegen;

        public OutputStomp(SlideGeneratorLib.SlideGenerator slidegen)
        {
            this.slidegen = slidegen;
        }


        internal void sendToStompProcess(String filename, String link){
            
            String radiovistxt = "";
            String radiovislink = link;
            try
            {
                radiovistxt = slidegen.cstlist["RADIOVISTXT"];
            }
            catch { }

            if (radiovislink == "")
            {
                radiovislink = CMSConfig.radiovislink;
            }
            if (radiovistxt == "")
                radiovistxt = CMSConfig.radiovistxt;
            String server = CMSConfig.stompserver;
            Int32 port = CMSConfig.stompport;
            String url = CMSConfig.stomphttpurl;

            
            
            
            
            foreach (String topic in CMSConfig.stomptopic)
            {
                String t = topic;
                if (!t.EndsWith("/"))
                    t = t + "/";
                Console.WriteLine("SEND STOMP : " + topic);

                Thread newThread2 = new Thread(new ParameterizedThreadStart(this.sendToStompThread));
                newThread2.Start(new StompParam(server, port, t + "image", radiovislink, "SHOW " + url + "" + filename));

                // TRICK
               /* Thread newThread3 = new Thread(new ParameterizedThreadStart(this.sendToStompThread));
                newThread3.Start(new StompParam(server, port, t + "text", radiovislink, "TEXT " + radiovistxt + ""));*/
            }
        }

        private void sendToStompThread(object s)
        {
            StompParam stomp = (StompParam)s;
            sendToStomp(stomp.address, stomp.port, stomp.topic, stomp.link, stomp.showparam);
        }

        private void sendToStomp(string address, int port, string topic, String link, String showparam)
        {
            try
            {
                
                Uri connecturi = new Uri("stomp:tcp://" + address + ":" + port+ "");

                Log.log("new stomp transaction with " + connecturi.ToString() + "\ttopic:" + topic + "\tmessage:" + showparam + "", "STOMP");

                // UIMain.errorAdd("[STOMP]  About to connect to " + connecturi);

                // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.

                IConnectionFactory factory = new NMSConnectionFactory(connecturi);

                using (IConnection connection = factory.CreateConnection(CMSConfig.stomplogin, CMSConfig.stomppasscode))
                using (ISession session = connection.CreateSession())
                {
                    


                    IDestination destination = SessionUtil.GetDestination(session, topic.Replace("/topic/", ""), DestinationType.Topic);
                    String topicListener = topic.ToString();
                    if(topic.Contains("*")){
                        Log.log("*");
                        if (topic.Contains("/topic/fm/"))
                        {
                            Log.log("fm");
                            topicListener = topic.Replace("*", "10320");
                        }
                        else if (topic.Contains("/topic/dab/"))
                        {
                            Log.log("dab");
                            topicListener = topic.Replace("*", "0");
                        }
                    }
                    IDestination destinationListener = SessionUtil.GetDestination(session, topicListener.Replace("/topic/", ""), DestinationType.Topic);
                     UIMain.errorAdd("[STOMP] Using destination: " + destinationListener);
                    //Console.WriteLine("[STOMP] " + destination + " " + showparam);
                    // Create a consumer and producer 
                    using (IMessageConsumer consumer = session.CreateConsumer(destinationListener))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        // Start the connection so that messages will be processed.
                        connection.Start();

                        producer.RequestTimeout = receiveTimeout;
                        consumer.Listener += new MessageListener(OnMessage);

                        

                        // Send a message
                        ITextMessage request = session.CreateTextMessage(showparam);
                        
                        
                    /*    request.NMSCorrelationID = topic+""+DateTime.Now.ToFileTime().ToString();
                        request.Properties["NMSXGroupID"] = "EBURADIO" + DateTime.Now.ToFileTime().ToString();
                        request.Properties["myHeader"] = "Cheddar";*/
                        if(link!="")
                        request.Properties["link"] = link;
                        request.Properties["trigger-time"] = "NOW";
                        
                        request.NMSPriority = MsgPriority.Highest;
                        request.NMSDeliveryMode = MsgDeliveryMode.NonPersistent;
                        request.NMSTimeToLive = TimeSpan.FromSeconds(15);
                        request.NMSDestination = destination;
                        request.Text = showparam;
                        Log.log("Send : " + showparam);
                        producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                       

                        producer.Send(request);
                        
                        

                        // Wait for the message
                        semaphore.WaitOne((int)receiveTimeout.TotalMilliseconds, true);
                        if (message == null)
                        {
                              Log.log(topic + "\tno message received from server", "STOMP");
                              UIMain.errorAdd("[STOMP] No message received!");
                        }
                        else
                        {
                            Log.log("callback:"+ topic + "\tmessage received from server: "+message.Text+"\n********", "STOMP");
                            UIMain.errorAdd("callback:" + topic + "\tmessage received from server: " + message.Text + "\n********", "STOMP");
                        }
                        producer.Close();
                        consumer.Close();
                    }
                    session.Close();
                    connection.Close();
                }

                Log.log(topic + "\tSTOMP OK", "STOMP");
            
            }
            catch (Exception e)
            {
                //UIMain.errorAdd("[STOMP] catch"); 
                Log.log("catch exception : "+e.Message+" "+e.StackTrace, "STOMP");
            }
        }

        private static AutoResetEvent semaphore = new AutoResetEvent(false);
        private static ITextMessage message = null;
        private static TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);
        protected static void OnMessage(IMessage receivedMsg)
        {
            message = receivedMsg as ITextMessage;
            semaphore.Set();

        }
    }
}
