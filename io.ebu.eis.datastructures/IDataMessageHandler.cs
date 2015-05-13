namespace io.ebu.eis.datastructures
{
    public interface IDataMessageHandler
    {
        void OnReceive(DataMessage message);
        void UpdateGlobalData(DataMessage message);
    }
}
