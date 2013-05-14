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
using System.Net.Sockets;
using System.Threading;
using ContentManager.GUI;
using System.Net;
using XMLConfig.CMS;
using System.Windows;

namespace ContentManager.Input.HTTP
{
    class InputHTTP : IInputPlugin
    {
        private TcpListener myListener;
        private int port = 80;
        private InputHTTPAction action;
        private ContentManagerCore core;

        public InputHTTP(ContentManagerCore core)
        {
            this.core = core;
            this.setup(null);
            this.start();
        }

        public void setup(Dictionary<string, string> variableList)
        {
            action = new InputHTTPAction(core);
            try
            {
                port = CMSConfig.httpport;
                myListener = new TcpListener(port);
            }
            catch (Exception e)
            {
                UIMain.fatalError("[InputHTTP] Unable to open port : "+port+"\n"+e.Message);
            }

        }

        public bool start()
        {
            try{
                myListener.Start();
                Thread th = new Thread(new ThreadStart(StartListen));
                th.Start();
                return true;
            }
            catch (Exception e)
            {
                UIMain.fatalError("[InputHTTP] Unable to open port : "+port+"\n"+e.Message);
                return false;
            }
        }

        public bool stop()
        {
            myListener.Stop();
            this.isRunning = false;
            return true;
        }

        public string getPluginType()
        {
            return "HTTP";
        }

        Boolean isRunning = true;
        public void StartListen()
        {
            while (isRunning)
            {

                try
                {
                    Socket mySocket = myListener.AcceptSocket();
                    Console.WriteLine("Socket Type " + mySocket.SocketType);
                    if (mySocket.Connected)
                    {
                        InputHTTPHandle httpHandle = new InputHTTPHandle(ref mySocket, action, core.slidegen.getAvailableSlides());
                        Thread t = new Thread(new ThreadStart(httpHandle.handleRequest));
                        t.Start();
                    }
                }
                catch(Exception e)
                {
                    
                }
                
            }
        }
    }
}
