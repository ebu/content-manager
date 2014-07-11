using io.ebu.eis.datastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.ebu.eis.mq
{
    public interface IAMQDataMessageHandler
    {
        void OnReceive(DataMessage message);
    }
}
