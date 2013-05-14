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
using XMLConfig.CMS;

using Kayak;
using ContentManager.GUI;
using Kayak.Http;
using System.Net;
using System.Threading;
using System.Windows;

namespace ContentManager.Input.HTTP
{
    class InputHTTPKayak : IInputPlugin
    {

        private ContentManagerCore core;
        private InputHTTPAction action;
        private IScheduler scheduler;
        private IServer server;
        private Thread serverThread;


        public InputHTTPKayak(ContentManagerCore core)
        {
            this.core = core;
            this.setup(null);
            //this.start();

            serverThread = new Thread(startThread);
            serverThread.Start();
            HttpRequestHead r;
        }


        public void setup(Dictionary<string, string> variableList)
        {
            action = new InputHTTPAction(core);
            try
            {
                //port = CMSConfig.httpport;
                scheduler = KayakScheduler.Factory.Create(new SchedulerDelegate());
                server = KayakServer.Factory.CreateHttp(new RequestDelegate(action), scheduler);
            }
            catch (Exception e)
            {
                UIMain.fatalError("[InputHTTP] Unable to open port : " + CMSConfig.httpport + "\n" + e.Message);
            }
        }

        public void startThread()
        {
            start();
        }

        public bool start()
        {
            try
            {
                using (server.Listen(new IPEndPoint(IPAddress.Any, CMSConfig.httpport)))
                {

                    // runs scheduler on calling thread. this method will block until
                    // someone calls Stop() on the scheduler.
                    scheduler.Start();
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error : " + e.Message);
                
            }
            return false;
        }

        public bool stop()
        {
            scheduler.Stop();
            return true;
        }

        public string getPluginType()
        {
            return "HTTP";
        }

        class SchedulerDelegate : ISchedulerDelegate
        {
            public void OnException(IScheduler scheduler, Exception e)
            {
                Console.WriteLine("Error on scheduler.");
                e.DebugStackTrace();
            }

            public void OnStop(IScheduler scheduler)
            {

            }
        }

        class RequestDelegate : IHttpRequestDelegate
        {
            private InputHTTPAction action;

            public RequestDelegate(InputHTTPAction action)
            {
                this.action = action;
            }
            //OK + buffered Body
            private void send200(HttpRequestHead request, IDataProducer requestBody, IHttpResponseDelegate response, String responseBody)
            {

                var headers = new HttpResponseHead()
                {
                    Status = "200 OK",
                    Headers = new Dictionary<string, string>() 
                    {
                        { "Content-Type", "text/plain" },
                        { "Content-Length", responseBody.Length.ToString() }
                    }
                };
                var body = new BufferedProducer(responseBody);
                response.OnResponse(headers, body);

            }
            //Not found
            private void send404(HttpRequestHead request, IDataProducer requestBody,
                IHttpResponseDelegate response)
            {
                var responseBody = "The resource you requested ('" + request.Uri + "') could not be found.";
                var headers = new HttpResponseHead()
                {
                    Status = "404 Not Found",
                    Headers = new Dictionary<string, string>()
                    {
                        { "Content-Type", "text/plain" },
                        { "Content-Length", responseBody.Length.ToString() }
                    }
                };
                var body = new BufferedProducer(responseBody);

                response.OnResponse(headers, body);
            }

            //Bad request
            private void send400(HttpRequestHead request, IDataProducer requestBody,
                IHttpResponseDelegate response, String msg)
            {
                var responseBody = "The resource you requested ('" + request.Uri + "') could not be executed.";
                if (!msg.Equals(""))
                    responseBody += "\r\nError: " + msg;
                var headers = new HttpResponseHead()
                {
                    Status = "400 Bad Request",
                    Headers = new Dictionary<string, string>()
                    {
                        { "Content-Type", "text/plain" },
                        { "Content-Length", responseBody.Length.ToString() }
                    }
                };

                var body = new BufferedProducer(responseBody);

                response.OnResponse(headers, body);
            }

            InputHTTPResult.INPUTCOMMAND getCommand(HttpRequestHead request)
            {

                if (request.Uri.StartsWith("/updateandbroadcast"))
                {
                    return InputHTTPResult.INPUTCOMMAND.UPDATEANDBROADCAST;
                }
                else if (request.Uri.StartsWith("/update"))
                {
                    return InputHTTPResult.INPUTCOMMAND.UPDATE;
                }
                else if (request.Uri.StartsWith("/broadcast"))
                {
                    return InputHTTPResult.INPUTCOMMAND.BROADCASTSLIDE;
                }
                else if (request.Uri.StartsWith("/loadslidecart"))
                {
                    return InputHTTPResult.INPUTCOMMAND.LOADSLIDECART;
                }
                else if (request.Uri.StartsWith("/system"))
                {
                    return InputHTTPResult.INPUTCOMMAND.SYSTEM;
                }
                else if (request.Uri.StartsWith("/change"))
                {
                    return InputHTTPResult.INPUTCOMMAND.CHANGE;
                }
                else if (request.Uri.StartsWith("/preview"))
                {
                    return InputHTTPResult.INPUTCOMMAND.PREVIEW;
                }
                else return InputHTTPResult.INPUTCOMMAND.NOTIMPLEMENTED;

            }

            public void OnRequest(HttpRequestHead request, IDataProducer requestBody,
                IHttpResponseDelegate response)
            {
                try
                {
                    InputHTTPResult.INPUTCOMMAND command = getCommand(request);
                    //POST
                    if (command != InputHTTPResult.INPUTCOMMAND.NOTIMPLEMENTED && command != InputHTTPResult.INPUTCOMMAND.ERROR && request.Method.ToUpperInvariant() == "POST")
                    {
                        requestBody.Connect(new BufferedConsumer(bufferedBody =>
                        {
                            Dictionary<String, String> parameters = new Dictionary<String, String>();
                            foreach (String k in bufferedBody.Split(new char[] { '&' }))
                            {
                                String[] kv = k.Split(new char[] { '=' });
                                if (kv.Length == 2)
                                    parameters.Add(kv[0], System.Web.HttpUtility.UrlDecode(kv[1]));
                            }


                            try
                            {
                                String msg = action.Execute(new InputHTTPResult(command, parameters));
                                if (msg.Equals(""))
                                    msg = "OK";
                                send200(request, requestBody, response, msg);
                            }
                            catch (Exception e)
                            {
                                send400(request, requestBody, response, e.Message);
                            }



                        }, error =>
                        {
                            send400(request, requestBody, response, "no POST variables");
                        }));
                    }
                        //GET
                    else if (command != InputHTTPResult.INPUTCOMMAND.NOTIMPLEMENTED && command != InputHTTPResult.INPUTCOMMAND.ERROR && request.Method.ToUpperInvariant() == "GET")
                    {
                        Dictionary<String, String> parameters = new Dictionary<String, String>();
                        foreach (String k in request.QueryString.Split(new char[] { '&' }))
                        {
                            String[] kv = k.Split(new char[] { '=' });
                            if (kv.Length == 2)
                                parameters.Add(kv[0], System.Web.HttpUtility.UrlDecode(kv[1]));

                        }


                        try
                        {
                            String msg = action.Execute(new InputHTTPResult(command, parameters));
                            if (msg.Equals(""))
                                msg = "OK";
                            send200(request, requestBody, response, msg);
                        }
                        catch (Exception e)
                        {
                            send400(request, requestBody, response, e.Message);
                        }


                    }
                    else
                    {
                        send404(request, requestBody, response);
                    }
                }
                catch (Exception e)
                {
                    send400(request, requestBody, response, e.Message);
                }
            }
        }

        class BufferedProducer : IDataProducer
        {
            ArraySegment<byte> data;

            public BufferedProducer(string data) : this(data, Encoding.UTF8) { }
            public BufferedProducer(string data, Encoding encoding) : this(encoding.GetBytes(data)) { }
            public BufferedProducer(byte[] data) : this(new ArraySegment<byte>(data)) { }
            public BufferedProducer(ArraySegment<byte> data)
            {
                this.data = data;
            }

            public IDisposable Connect(IDataConsumer channel)
            {
                // null continuation, consumer must swallow the data immediately.
                channel.OnData(data, null);
                channel.OnEnd();
                return null;

            }
        }

        class BufferedConsumer : IDataConsumer
        {
            List<ArraySegment<byte>> buffer = new List<ArraySegment<byte>>();
            Action<string> resultCallback;
            Action<Exception> errorCallback;

            public BufferedConsumer(Action<string> resultCallback,
                Action<Exception> errorCallback)
            {
                this.resultCallback = resultCallback;
                this.errorCallback = errorCallback;
            }
            public bool OnData(ArraySegment<byte> data, Action continuation)
            {
                try
                {
                    // since we're just buffering, ignore the continuation. 
                    // TODO: place an upper limit on the size of the buffer. 
                    // don't want a client to take up all the RAM on our server! 
                    buffer.Add(data);
                    return false;
                }
                catch
                {
                    Log.log("Connect error", "HTTPinput");
                    return false;
                }
            }
            public void OnError(Exception error)
            {
                errorCallback(error);
            }

            public void OnEnd()
            {
                // turn the buffer into a string. 
                // 
                // (if this isn't what you want, you could skip 
                // this step and make the result callback accept 
                // List<ArraySegment<byte>> or whatever) 
                // 
                var str = buffer
                    .Select(b => Encoding.UTF8.GetString(b.Array, b.Offset, b.Count))
                    .Aggregate((result, next) => result + next);

                resultCallback(str);
            }
        }
    }
}