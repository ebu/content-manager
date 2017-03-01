using System.Collections.Generic;

namespace io.ebu.eis.datastructures
{
    public interface IDataMessageHandler
    {
        void OnReceive(DataMessage message);
        void UpdateGlobalData(DataMessage message);

        void BroadcastSlide(string slidename);

        void ClearActiveCart();

        void AddSlides(List<string> slideNames);

        bool HandleWorkerTask(string message);
    }
}
