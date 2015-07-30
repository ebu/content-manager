using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace io.ebu.eis.contentmanager
{
    internal static class JsonSerializer
    {
        public static string Serialize<T>(T obj)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var ms = new MemoryStream();
            serializer.WriteObject(ms, obj);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static T Deserialize<T>(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var byteArray = Encoding.UTF8.GetBytes(json);
            var ms = new MemoryStream(byteArray);
            var entry = (T)serializer.ReadObject(ms);
            ms.Close();

            return entry;
        }
    }
}
