using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.ebu.eis.data.file
{
    public interface ISystemFileRouter
    {
        void RouteFile(string filepath);
    }
}
