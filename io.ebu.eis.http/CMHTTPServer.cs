using System;
using System.Net;
using System.Threading;
using io.ebu.eis.datastructures;

namespace io.ebu.eis.http
{
    public class CMHttpServer: IDisposable
    {
        private readonly string _bindIp;
        private readonly int _bindPort;

        private readonly IDataMessageHandler _handler;

        private HttpListener _httpListener;

        public CMHttpServer(string bindIp, int bindPort, IDataMessageHandler handler)
        {
            _bindIp = bindIp;
            _bindPort = bindPort;
            _handler = handler;
        }

        public void Start()
        {
            try
            {
                _httpListener = new HttpListener();

                // Add Prefixes
                var slisten = "http://" + _bindIp + ":" + _bindPort + "/";
                _httpListener.Prefixes.Add(slisten); //"http://localhost:8080/"

                // Start Listener
                _httpListener.Start();

                // Start Processing Thread
                var t = new Thread(Process);
                t.Start();
            }
            catch (HttpListenerException)
            {
                // TODO Report
            }
        }

        public void Stop()
        {
            if(_httpListener != null)
                _httpListener.Stop();
        }

        private void Process()
        {
            while (_httpListener != null && _httpListener.IsListening)
            {
                try
                {
                    HttpListenerContext request = _httpListener.GetContext();

                    var h = new CMHttpHandler(request, _handler);
                    var t = new Thread(h.ProcessRequest);
                    t.Start();

                }
                catch (HttpListenerException)
                {
                    // TODO LOG
                }
                catch (InvalidOperationException)
                {
                    // TODO LOG
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }

}
