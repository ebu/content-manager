using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using io.ebu.eis.shared;

namespace io.ebu.eis.shared
{
    public interface IImageGenerationHandler
    {
        void DispatchGeneration(string id, long serial, ManagerImageReference image);
        void RegisterImageCallback(string id, long serial, ManagerImageReference image);

    }
}
