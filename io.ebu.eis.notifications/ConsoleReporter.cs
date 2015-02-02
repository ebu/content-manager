using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace io.ebu.eis.notifications
{
    public class ConsoleReporter : IReporter
    {
        private readonly Queue<string> _infoQueue;
        private bool _running;

        public ConsoleReporter()
        {
            _infoQueue = new Queue<string>();
            _running = true;

            var asdThread = new Thread(Process);
            asdThread.Start();
        }
        ~ConsoleReporter()
        {
            Stop();
        }

        public void Stop()
        {
            lock (this)
            {
                _running = false;
                Monitor.PulseAll(this);
            }
        }


        private void Process()
        {
            lock (this)
            {
                while (_running)
                {
                    if (_infoQueue.Any())
                    {
                        try
                        {
                            while (_infoQueue.Any())
                            {
                                writeToConsole(_infoQueue.Dequeue());
                            }
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch (Exception) { }
                    }

                    Monitor.Wait(this);
                }
            }

        }

        private void writeToConsole(string message)
        {
            Console.WriteLine(DateTime.Now + " : " + message);
        }


        public void NotifyException(Exception e, NotificationLevel level)
        {
            NotifyException(e, e.Message, level);
        }

        public void NotifyException(Exception e, string m, NotificationLevel level)
        {
            lock (this)
            {
                _infoQueue.Enqueue(level.ToString().ToUpper() + " EXCEPTION OCCURED : " + e.Message + "\n" + e.StackTrace);
                int i = 0;
                while (i < 10 && e.InnerException != null)
                {
                    e = e.InnerException;
                    ++i;
                    _infoQueue.Enqueue("   INNER EXCEPTION : " + e.Message + "\n" + e.StackTrace);
                }
                Monitor.PulseAll(this);
            }
        }

        public void NotifyMessage(string m, NotificationLevel level)
        {
            lock (this)
            {
                _infoQueue.Enqueue(level.ToString().ToUpper().PadLeft(6, ' ') + " : " + m);
                Monitor.PulseAll(this);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
