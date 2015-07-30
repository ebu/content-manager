using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using io.ebu.eis.datastructures;

namespace io.ebu.eis.http
{
    internal class CMHttpHandler
    {
        private static readonly Regex UpdateRex = new Regex(@"\/update(\/)?\?(?<values>.*)$");
        private static readonly Regex BroadcastRex = new Regex(@"\/broadcast(\/)?\?(?<values>.*)$");
        private static readonly Regex UpdateAndBroadcastRex = new Regex(@"\/updateandbroadcast(\/)?\?(?<values>.*)$");
        private static readonly Regex LoadRex = new Regex(@"\/loadslidecart(\/)?\?(?<values>.*)$");
        private static readonly Regex PreviewRex = new Regex(@"\/preview(\/)?\?(?<values>.*)$");
        private static readonly Regex SystemRex = new Regex(@"\/system(\/)?\?(?<values>.*)$");
        private static readonly Regex ChangeRex = new Regex(@"\/change(\/)?\?(?<values>.*)$");

        // IceCast Update look like : http://192.168.1.10:8000/admin/metadata?mount=/mystream&mode=updinfo&song=ACDC+Back+In+Black
        private static readonly Regex IceCastRex = new Regex(@"\/admin/metadata(\/)?\?(?<values>.*)");

        private enum CMCommand { Update, Broadcast, UpdateAndBroadcast, Load, Preview, System, Change, IceCast }

        private readonly HttpListenerContext _context;
        private readonly IDataMessageHandler _handler;
        internal CMHttpHandler(HttpListenerContext context, IDataMessageHandler handler)
        {
            _context = context;
            _handler = handler;
        }

        internal void ProcessRequest()
        {

            try
            {
                string responseString = "";

                // Manage Encoding
                _context.Response.ContentEncoding = _context.Request.ContentEncoding;
                // Set CM Server Headers
                _context.Response.Headers.Add("Server", "EBU Content Manager");

                if (_context.Request.HttpMethod == "GET")
                {
                    // GET Request
                    var kvStore = _context.Request.QueryString;
                    switch (RawUrlToCMCommand(_context.Request.RawUrl))
                    {
                        case CMCommand.Update:
                        case CMCommand.IceCast:
                            var dm = new DataMessage
                            {
                                Key = "GLOBAL",
                                DataType = "GLOBAL",
                                Value = "Global Message"
                            };
                            foreach (var k in kvStore.AllKeys)
                            {
                                var d = new DataMessage
                                {
                                    Key = k,
                                    Value = kvStore[k],
                                    DataType = "STRING"
                                };
                                dm.Data.Add(d);
                            }
                            _handler.UpdateGlobalData(dm);
                            _context.Response.StatusCode = 200;
                            responseString = "Ok.";
                            break;
                        // TODO Add additional commands
                        default:
                            _context.Response.StatusCode = 404;
                            responseString = "GET Requests generally are not supported.";
                            break;
                    }

                    // TODO For Future use of status return
                    //_context.Response.ContentType = "application/json";
                    //_context.Response.StatusCode = 200;
                    //responseString = ""; //GetJSONStatus();

                    // Write Response
                    var buffer = _context.Request.ContentEncoding.GetBytes(responseString);
                    _context.Response.Close(buffer, false);

                }
                else if (_context.Request.HttpMethod == "POST")
                {
                    if (_context.Request.HasEntityBody)
                    {
                        try
                        {
                            // Read Incoming Content
                            var body = _context.Request.InputStream;
                            var reader = new StreamReader(body, _context.Request.ContentEncoding);
                            var requestString = reader.ReadToEnd();
                            body.Close();
                            reader.Close();

                            // Handle the Incoming Content
                            var data = DataMessage.Deserialize(requestString);
                            // Dispatch Data
                            _handler.OnReceive(data);

                            // Responses
                            _context.Response.ContentType = "application/json";
                            responseString = "{\"status\":\"ok\",\"receivedtype\":\"" + data.DataType +
                                "\",\"receiveddatacount\":" + data.Data.Count + "}";

                            // Write Response
                            var buffer = _context.Request.ContentEncoding.GetBytes(responseString);
                            _context.Response.StatusCode = 200;
                            _context.Response.Close(buffer, false);

                        }
                        catch (Exception e)
                        {
                            // TODO Log this
                            _context.Response.StatusCode = 500;
                            var buffer = _context.Request.ContentEncoding.GetBytes("Error occured.\n" + e.Message);
                            _context.Response.Close(buffer, false);
                        }
                    }
                    else
                    {
                        _context.Response.StatusCode = 400;
                        var buffer = _context.Request.ContentEncoding.GetBytes("POST should contain body content.");
                        _context.Response.Close(buffer, false);
                    }
                }
                else
                {
                    // Generally Unsupported Method
                    _context.Response.StatusCode = 406;
                    var buffer = _context.Request.ContentEncoding.GetBytes(_context.Request.HttpMethod + " method not supported.");
                    _context.Response.Close(buffer, false);
                }
            }
            catch (Exception ex)
            {
                // TODO Log this
                try
                {
                    _context.Response.StatusCode = 500;
                    var buffer = _context.Request.ContentEncoding.GetBytes("Error occured. " + ex.Message);
                    _context.Response.Close(buffer, false);
                }
                catch (Exception)
                {
                    // TODO Log
                }
            }
        }

        private static CMCommand RawUrlToCMCommand(string rawUrl)
        {
            if (UpdateRex.Match(rawUrl).Success)
                return CMCommand.Update;
            if (BroadcastRex.Match(rawUrl).Success)
                return CMCommand.Broadcast;
            if (UpdateAndBroadcastRex.Match(rawUrl).Success)
                return CMCommand.UpdateAndBroadcast;
            if (LoadRex.Match(rawUrl).Success)
                return CMCommand.Load;
            if (PreviewRex.Match(rawUrl).Success)
                return CMCommand.Preview;
            if (SystemRex.Match(rawUrl).Success)
                return CMCommand.System;
            if (ChangeRex.Match(rawUrl).Success)
                return CMCommand.Change;
            if (IceCastRex.Match(rawUrl).Success)
                return CMCommand.IceCast;
            return CMCommand.System;
        }
    }
}
