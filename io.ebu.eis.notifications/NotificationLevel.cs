using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.ebu.eis.notifications
{
    /// <summary>
    /// Indicates the importance of the notification.
    /// </summary>
    public enum NotificationLevel
    {
        /// <summary>
        /// The error is fatal and more severe than a captured exception or regular smpag.wfs.shared.NotificationLevel.Error.
        /// Errors at this severity will show up as dark red in the Sentry Stream.
        /// </summary>
        Fatal = 0,
        /// <summary>
        /// The error is of the same severity as a captured exception. Errors at this
        /// severity will show up as bright red in the Sentry Stream.
        /// </summary>
        Error = 1,
        /// <summary>
        /// The error is less severe than an a regular smpag.wfs.shared.NotificationLevel.Error.
        /// Errors at this severity will show up as orange in the Sentry Stream.
        /// </summary>
        Warning = 2,
        /// <summary>
        /// The error is less severe than a smpag.wfs.shared.NotificationLevel.Warning and is
        /// probably expected. 
        /// Errors at this severity will show up as blue in the Sentry Stream.
        /// </summary>
        Info = 3,
        /// <summary>
        /// The error is less even severe than an smpag.wfs.shared.NotificationLevel.Info and
        /// is just captured for debug purposes. Errors at this severity will show up
        /// as grey in the Sentry Stream.
        /// </summary>
        Debug = 4,
    }
}
