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
using ContentManager.GUI;
using System.Windows;
using XMLConfig.CMS;

namespace ContentManager.Input.HTTP
{
    public class InputHTTPHandle
    {
        InputHTTPAction action;

        Socket mySocket;
        String sHttpVersion = "";
        private List<string> availableSlides;

        public InputHTTPHandle(ref Socket mySocket, InputHTTPAction action, List<String> availableSlides)
        {
            this.mySocket = mySocket;
            this.action = action;
            this.availableSlides = availableSlides;
        }

        public void handleRequest()
        {
            Console.WriteLine("Client IP {0}\n", mySocket.RemoteEndPoint);

            Byte[] bReceive = new Byte[1024];
            int i = mySocket.Receive(bReceive, bReceive.Length, 0);

            string sBuffer = Encoding.ASCII.GetString(bReceive);
            
            InputHTTPResult res;

            if (sBuffer.Substring(0, 3) == "GET")
            {
                res = parseGETRequest(sBuffer);
            }
            else if (sBuffer.Substring(0, 4) == "POST")
            {
                res = parsePOSTRequest(sBuffer);
            }
            else
            {
                Console.WriteLine("Only Get Method is supported..");
                mySocket.Close();
                return;
            }
            

            if (res.command == InputHTTPResult.INPUTCOMMAND.NOTIMPLEMENTED)
            {
                String msg = "Not implemented";
                this.SendHeader(this.sHttpVersion, "text/plain", msg.Length, " 404 Not Found", ref mySocket);
                this.SendToBrowser(msg, ref mySocket);
            }
            else if (res.command == InputHTTPResult.INPUTCOMMAND.ERROR)
            {
                String msg = "Exception: Bad Request Synthax";
                this.SendHeader(this.sHttpVersion, "text/plain", msg.Length, " 404 Not Found", ref mySocket);
                this.SendToBrowser(msg, ref mySocket);
            }
            else
            {
                String msg = "OK";
                switch (res.command)
                {
                    case InputHTTPResult.INPUTCOMMAND.UPDATE:
                        action.update(res.parameters);
                        break;
                    case InputHTTPResult.INPUTCOMMAND.UPDATEANDBROADCAST:
                        if (res.parameters.ContainsKey("BSLIDE") && res.parameters["BSLIDE"] != "")
                        {
                            if (availableSlides.Contains(res.parameters["BSLIDE"]))
                                msg = action.updateAndBroadcast(res.parameters, res.parameters["BSLIDE"]);
                            else
                                msg = "SLIDE NOT FOUND (" + res.parameters["BSLIDE"]+")";
                        }
                        else
                        {
                            action.update(res.parameters);
                        }
                        break;
                    case InputHTTPResult.INPUTCOMMAND.BROADCASTSLIDE:
                        if (res.parameters.ContainsKey("BSLIDE") && res.parameters["BSLIDE"] != "")
                        {
                            if (availableSlides.Contains(res.parameters["BSLIDE"]))
                                msg = action.broadcast(res.parameters["BSLIDE"]);
                            else
                                msg = "SLIDE NOT FOUND (" + res.parameters["BSLIDE"]+")";
                        }
                        else
                        {
                            msg = "Broadcast BAD FORMAT: BSLIDE argument is missing.";
                        }
                        break;
                    case InputHTTPResult.INPUTCOMMAND.CHANGE:
                        msg = action.change(res.parameters);
                        break;
                    case InputHTTPResult.INPUTCOMMAND.LOADSLIDECART:
                        msg = action.loadslidecart(res.parameters);
                        break;
                    case InputHTTPResult.INPUTCOMMAND.SYSTEM:
                        msg = action.system(res.parameters);
                        break;
                    case InputHTTPResult.INPUTCOMMAND.PREVIEW:
                        if (res.parameters.ContainsKey("SLIDE") && res.parameters["SLIDE"] != "")
                        {
                            if (availableSlides.Contains(res.parameters["SLIDE"]))
                                msg = action.preview(res.parameters["SLIDE"], res.parameters);
                            else
                                msg = "ERROR: SLIDE "+res.parameters["SLIDE"]+" is not available";
                        }
                        else
                            msg = "ERROR: SLIDE argument is missing";
                        break;
                }
                
                this.SendHeader(this.sHttpVersion, "text/plain", msg.Length, " 200 Ok", ref mySocket);
                this.SendToBrowser(msg, ref mySocket);
            }

            mySocket.Close();
        }
        public void dispatchAction(){

        }

        public void SendHeader(string sHttpVersion, string sMIMEHeader, int iTotBytes, string sStatusCode, ref Socket mySocket)
        {
            String sBuffer = "";
            if (sMIMEHeader.Length == 0)
            {
                sMIMEHeader = "text/plain"; // Default Mime Type text/plain
            }
            sBuffer = sBuffer + sHttpVersion + sStatusCode + "\r\n";
            sBuffer = sBuffer + "Server: EBU-RadioProductionPlatform\r\n";
            sBuffer = sBuffer + "Content-Type: " + sMIMEHeader + "\r\n";
            sBuffer = sBuffer + "Accept-Ranges: bytes\r\n";
            sBuffer = sBuffer + "Content-Length: " + iTotBytes + "\r\n\r\n";
            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);
            SendToBrowser(bSendData, ref mySocket);
        }

        public InputHTTPResult parseGETRequest(String sBuffer)
        {
            string sRequest = "";
            try
            {
                int iStartPos = sBuffer.IndexOf("HTTP", 1);
                this.sHttpVersion = sBuffer.Substring(iStartPos, sBuffer.IndexOf("\n", iStartPos));
                sRequest = sBuffer.Substring(0, iStartPos - 1);

                sRequest = sRequest.Substring(sRequest.Substring(0, sRequest.IndexOf('?')).LastIndexOf("/")+1);

                String[] s = sRequest.Split(("?").ToArray(), 2, StringSplitOptions.None);
                
                InputHTTPResult.INPUTCOMMAND cmd = InputHTTPResult.INPUTCOMMAND.NOTIMPLEMENTED;
                switch (s[0].ToLower())
                {
                    case "update":
                        cmd = InputHTTPResult.INPUTCOMMAND.UPDATE;
                        break;
                    case "updateandbroadcast":
                        cmd = InputHTTPResult.INPUTCOMMAND.UPDATEANDBROADCAST;
                        break;
                    case "broadcast":
                        cmd = InputHTTPResult.INPUTCOMMAND.BROADCASTSLIDE;
                        break;
                    case "loadslidecart":
                        cmd = InputHTTPResult.INPUTCOMMAND.LOADSLIDECART;
                        break;
                    case "system":
                        cmd = InputHTTPResult.INPUTCOMMAND.SYSTEM;
                        break;
                    case "change":
                        cmd = InputHTTPResult.INPUTCOMMAND.CHANGE;
                        break;
                    case "preview":
                        cmd = InputHTTPResult.INPUTCOMMAND.PREVIEW;
                        break;
                  /*  case "changedir":
                        cmd = InputHTTPResult.INPUTCOMMAND.CHANGEDIR;
                        break;*/
                    case "":                        
                    default:
                        cmd = InputHTTPResult.INPUTCOMMAND.NOTIMPLEMENTED;
                        break;
                }


                char separator = '|';
                String separatorParameter = CMSConfig.httpurlseparator;
                if(separatorParameter != null){
                    if (separatorParameter.Length != 1)
                    {
                        MessageBox.Show("Configuration key httpUrlSeparator : Bad format");
                        Environment.Exit(1);
                    }
                    else
                        separator = separatorParameter.ToCharArray().First();
                }
                String[] s2 = s[1].Split(separator);


                Dictionary<String, String> parameters = new Dictionary<String, String>();
                for (int i = 0; i < s2.Count(); i++)
                {
                    String[] p = s2[i].Split(("=").ToArray(), 2);
                    if(p.Length == 2)
                        parameters.Add(p[0], System.Web.HttpUtility.UrlDecode(p[1]));
                    
                }
                return new InputHTTPResult(cmd, parameters);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error when parsing request:"+sRequest+" "+e.Message);
                return new InputHTTPResult(InputHTTPResult.INPUTCOMMAND.ERROR, null);
            }

        }

        public InputHTTPResult parsePOSTRequest(String sBuffer)
        {
            string sRequest = "";
            try
            {
                int iStartPos = sBuffer.IndexOf("HTTP", 1);
                this.sHttpVersion = sBuffer.Substring(iStartPos, sBuffer.IndexOf("\n", iStartPos));
                sRequest = sBuffer.Substring(0, iStartPos - 1);

                sRequest = sRequest.Substring(sRequest.Substring(0, sRequest.IndexOf('?')).LastIndexOf("/") + 1);

                String[] s = sRequest.Split(("?").ToArray(), 2, StringSplitOptions.None);

                InputHTTPResult.INPUTCOMMAND cmd = InputHTTPResult.INPUTCOMMAND.NOTIMPLEMENTED;
                switch (s[0].ToLower())
                {
                    case "update":
                        cmd = InputHTTPResult.INPUTCOMMAND.UPDATE;
                        break;
                    case "updateandbroadcast":
                        cmd = InputHTTPResult.INPUTCOMMAND.UPDATEANDBROADCAST;
                        break;
                    case "broadcast":
                        cmd = InputHTTPResult.INPUTCOMMAND.BROADCASTSLIDE;
                        break;
                    case "loadslidecart":
                        cmd = InputHTTPResult.INPUTCOMMAND.LOADSLIDECART;
                        break;
                    case "system":
                        cmd = InputHTTPResult.INPUTCOMMAND.SYSTEM;
                        break;
                    case "change":
                        cmd = InputHTTPResult.INPUTCOMMAND.CHANGE;
                        break;
                    case "preview":
                        cmd = InputHTTPResult.INPUTCOMMAND.PREVIEW;
                        break;
                    /*  case "changedir":
                          cmd = InputHTTPResult.INPUTCOMMAND.CHANGEDIR;
                          break;*/
                    case "":
                    default:
                        cmd = InputHTTPResult.INPUTCOMMAND.NOTIMPLEMENTED;
                        break;
                }


                char separator = '|';
                String separatorParameter = CMSConfig.httpurlseparator;
                if (separatorParameter != null)
                {
                    if (separatorParameter.Length != 1)
                    {
                        MessageBox.Show("Configuration key httpUrlSeparator : Bad format");
                        Environment.Exit(1);
                    }
                    else
                        separator = separatorParameter.ToCharArray().First();
                }
                String[] s2 = s[1].Split(separator);


                Dictionary<String, String> parameters = new Dictionary<String, String>();
                for (int i = 0; i < s2.Count(); i++)
                {
                    String[] p = s2[i].Split(("=").ToArray(), 2);
                    if (p.Length == 2)
                        parameters.Add(p[0], System.Web.HttpUtility.UrlDecode(p[1]));

                }
                return new InputHTTPResult(cmd, parameters);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error when parsing request:" + sRequest + " " + e.Message);
                return new InputHTTPResult(InputHTTPResult.INPUTCOMMAND.ERROR, null);
            }

        }


        public void SendToBrowser(String sData, ref Socket mySocket)
        {
            SendToBrowser(Encoding.ASCII.GetBytes(sData), ref mySocket);
        }

        public void SendToBrowser(Byte[] bSendData, ref Socket mySocket)
        {
            int numBytes = 0; try
            {
                if (mySocket.Connected) { if ((numBytes = mySocket.Send(bSendData, bSendData.Length, 0)) == -1) Console.WriteLine("Socket Error cannot Send Packet"); else { } }
                else Console.WriteLine("Connection Dropped....");
            }
            catch (Exception e) { Console.WriteLine("Error Occurred : {0} ", e); }
        }

    }

    public class InputHTTPResult
    {
        public enum INPUTCOMMAND { UPDATE, UPDATEANDBROADCAST, BROADCASTSLIDE, LOADSLIDECART, CHANGE, PREVIEW, SYSTEM, ERROR, NOTIMPLEMENTED }
        public Dictionary<String, String> parameters = new Dictionary<String, String>();
        public INPUTCOMMAND command;

        public InputHTTPResult(INPUTCOMMAND command, Dictionary<String,String> parameters){

            this.command = command;
            this.parameters = parameters;
            
        }
    }

}
