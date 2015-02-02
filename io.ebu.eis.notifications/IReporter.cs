using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.ebu.eis.notifications
{
    public interface IReporter : IDisposable
    {
        void NotifyException(Exception e, NotificationLevel level);
        void NotifyException(Exception e, string m, NotificationLevel level);
        void NotifyMessage(string m, NotificationLevel level);
    }
}
