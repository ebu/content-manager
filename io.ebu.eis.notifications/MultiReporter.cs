using System;
using System.Collections.Generic;

namespace io.ebu.eis.notifications
{
    /// <summary>
    /// Dispatches the notifications to multiple IReporters
    /// </summary>
    public class MultiReporter : IReporter
    {
        private readonly List<IReporter> _reporters;
        public List<IReporter> Reporters { get { return _reporters; } }

        public MultiReporter()
        {
            _reporters = new List<IReporter>();
        }

        public void Add(IReporter reporter)
        {
            _reporters.Add(reporter);
        }
        public void Remove(IReporter reporter)
        {
            _reporters.Remove(reporter);
        }
        
        public void NotifyException(Exception e, NotificationLevel level)
        {
            foreach (var r in _reporters)
            {
                r.NotifyException(e, level);
            }
        }

        public void NotifyException(Exception e, string m, NotificationLevel level)
        {
            foreach (var r in _reporters)
            {
                r.NotifyException(e, m, level);
            }
        }

        public void NotifyMessage(string m, NotificationLevel level)
        {
            foreach (var r in _reporters)
            {
                r.NotifyMessage(m, level);
            }
        }

        public void Dispose()
        {
            foreach (var r in _reporters)
            {
                r.Dispose();
            }
        }
    }
}
