using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using io.ebu.eis.datastructures;

namespace io.ebu.eis.http
{
    internal class CMHttpHandler
    {
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
                _context.Response.Headers.Add("Server", "EBU.io Content Manager");


                if (_context.Request.HttpMethod == "GET")
                {
                    // GET Request

                    if (_context.Request.HttpMethod == "GET")
                    {
                        if (_context.Request.RawUrl == "/")
                        {
                            _context.Response.StatusCode = 404;
                            responseString = "GET Requests generally are not supported.";
                        }
                        else if (_context.Request.RawUrl == "/json/stats")
                        {
                            _context.Response.ContentType = "application/json";
                            _context.Response.StatusCode = 200;
                            responseString = ""; //GetJSONStats();
                        }
                        else
                        {
                            _context.Response.StatusCode = 404;
                            responseString = "GET Requests generally are not supported.";
                        }


                        // Write Response
                        var buffer = _context.Request.ContentEncoding.GetBytes(responseString);
                        _context.Response.Close(buffer, false);
                    }
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
                            _context.Response.Close();
                        }
                        finally
                        {
                            //
                        }
                    }
                    else
                    {
                        _context.Response.StatusCode = 400;
                        var buffer = _context.Request.ContentEncoding.GetBytes("POST should contain body content.");
                        _context.Response.Close(buffer, false);
                        _context.Response.Close();
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
            catch (Exception e)
            {
                // TODO Log this
            }
            finally
            {
                //
            }
        }
    }
}
