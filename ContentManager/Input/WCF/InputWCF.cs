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
using ContentServiceLibrary;


namespace ContentManager
{
    /// <summary>
    /// InputWCF is used for synchronization with EBU Radio Production Platform
    /// </summary>
    class InputWCF : IInputPlugin
    {
        public static ServiceHost host;
        public static String url = "net.tcp://localhost:8095/ContentService";
        private ContentManagerCore contentManagerCore;

        public InputWCF(){}

        public InputWCF(ContentManagerCore contentManagerCore)
        {
            this.contentManagerCore = contentManagerCore;

            this.setup(null);
            this.start();
        }

        void host_Faulted(object sender, EventArgs e)
        {
            Console.WriteLine("Connection error");
        }

        void host_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("connection ok!");
        }

        public void setup(Dictionary<string, string> variableList)
        {
            //variableList not used directly
            //this.url ......
            
        }

        public bool start()
        {
            Console.WriteLine("Opening ContentEngineService...");
            Type serviceType = typeof(IContentService);
            
            try
            {
                host = new ServiceHost(typeof(ContentService));
                host.Opened += new EventHandler(host_Opened);
                host.Faulted += new EventHandler(host_Faulted);
                host.AddServiceEndpoint(typeof(IContentService), new NetTcpBinding(), url);
                host.Open();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("WCF: Error when opening socket\n"+e.Message);
                return false;
            }
            
        }

        public bool stop()
        {
            try
            {
                host.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public string getPluginType()
        {
            return "WCF";
        }
    }
}
