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
using System.ServiceModel;

namespace ContentServiceLibrary
{
    public class ContentServiceConnection
    {
        public IContentService service = null;
        public ICommunicationObject channel;
        private DuplexChannelFactory<IContentService> duplex = null;

        public void connect(String url, IContentServiceCallback callback, EventHandler openedEvt = null, EventHandler faultEvt = null) //url = "net.tcp://localhost:8080/AudioService"
        {
            try
            {
                duplex = new DuplexChannelFactory<IContentService>(callback, new NetTcpBinding(), new EndpointAddress(url));
                
                service = duplex.CreateChannel();
                channel = (ICommunicationObject)service;
                IClientChannel c = (IClientChannel)channel;
                
                c.OperationTimeout = TimeSpan.FromSeconds(5);
                
                channel.Opened += new EventHandler(delegate(object o, EventArgs e)
                {
                    Console.WriteLine("[CONTENTSERVICE] Connection ok!");
                });

                if (openedEvt != null)
                    channel.Opened += openedEvt;
                if (faultEvt != null)
                    channel.Faulted += faultEvt;
                channel.Faulted += new EventHandler(delegate(object o, EventArgs e)
                {
                    Console.WriteLine("[CONTENTSERVICE] Connection lost");
                });

                channel.Closed += new EventHandler(delegate(object o, EventArgs e)
                {
                    Console.WriteLine("[CONTENTSERVICE] Connection closed");
                });

            }
            catch (Exception e)
            {
                Console.WriteLine("[CONTENTSERVICE] Connection error: " + e.Message);
            }
        }

        public void disconnect()
        {
            ((ICommunicationObject)service).Close();
            duplex.Close();
        }

        
    }
}
