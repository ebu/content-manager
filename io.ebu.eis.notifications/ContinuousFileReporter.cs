using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace io.ebu.eis.notifications
{
    /// <summary>
    /// ContinuousFileReporter write log file and uses internal queue to make sure all info are written in line to disk
    /// .log file are writte either to the Application's directory or to the users %TEMP% directory.
    /// </summary>
    public class ContinuousFileReporter : IReporter
    {
        private readonly string _relpath;
        private readonly string _logname;
        private string _currentFilename;
        private readonly Queue<string> _infoQueue;
        private bool _running;
        private readonly bool _dateBasedFileNames;
        private string _date = "";

        /// <summary>
        /// Default constructor. Will write logs to a folder named log in the applications directory.
        /// </summary>
        public ContinuousFileReporter() : this("", true) { }
        /// <summary>
        /// Constructor. Will Write log file to a named folder in the applications directory.
        /// </summary>
        /// <param name="logName">The name of the log folder</param>
        /// <param name="dateBasedFileNames">If true, will prepend yyyyMMdd to the log filenames</param>
        public ContinuousFileReporter(string logName, bool dateBasedFileNames) : this(logName, dateBasedFileNames, false) { }
        /// <summary>
        /// Constructor. Will write log files to a named folder either in temp or the application's folder.
        /// </summary>
        /// <param name="logName">The name of the log folder</param>
        /// <param name="dateBasedFileNames">If true, will prepend yyyyMMdd to the log filenames</param>
        /// <param name="tempFolder">If true, will write the logs to the users %TEMP% directory</param>
        public ContinuousFileReporter(string logName, bool dateBasedFileNames, bool tempFolder)
        {
            _logname = logName;
            _infoQueue = new Queue<string>();
            _dateBasedFileNames = dateBasedFileNames;
            

            if (tempFolder)
            {
                try
                {
                    _relpath = Path.GetTempPath() + logName + @" - log";
                }
                catch (Exception)
                {
                    _relpath = logName + @"log";
                }
            }
            else
            {
                _relpath = logName + @"log";
            }
            _currentFilename = _logname + ".log";
            UpdateFileNames();

            var asdThread = new Thread(Process);
            asdThread.Start();
        }
        ~ContinuousFileReporter()
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

        private void UpdateFileNames()
        {
            if (_dateBasedFileNames)
            {
                var date = DateTime.Now.ToString("yyyyMMdd");
                if (String.Compare(date, _date, StringComparison.Ordinal) != 0)
                {
                    // Date Changed... New Filename
                    _date = date;
                    _currentFilename = date + " " + _logname + ".log";
                }
            }
        }


        private void Process()
        {
            _running = true;

            if (!Directory.Exists(_relpath))
                Directory.CreateDirectory(_relpath);

            lock (this)
            {
                while (_running)
                {
                    UpdateFileNames();
                    if (_infoQueue.Any())
                    {
                        try
                        {
                            var infoWriter = new FileStream(Path.Combine(_relpath, _currentFilename), FileMode.Append, FileAccess.Write, FileShare.Read);
                            var infoSw = new StreamWriter(infoWriter);
                            while (_infoQueue.Any())
                            {
                                writeToSR(infoSw, _infoQueue.Dequeue());
                            }
                            infoSw.Flush();
                            infoSw.Close();
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch (Exception)
                        { }
                    }
                    Monitor.Wait(this);
                }
            }

        }

        private void writeToSR(StreamWriter sr, string message)
        {
            sr.WriteLine(DateTime.Now + " : \t" + message);
            sr.Flush();
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
