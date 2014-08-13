using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace io.ebu.eis.datastructures
{
    /// <summary>
    /// Matches the DataMessage case class in the DataGateway project
    /// https://github.com/ebu/DataGateway
    /// </summary>
    [DataContract]
    public class DataMessage
    {

        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "datatype")]
        public string DataType { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }

        [DataMember(Name = "data")]
        public List<DataMessage> Data { get; set; }


        public String GetValue(String path)
        {
            var splitPath = path.Split('.');

            // TODO Enhance
            // Evaluate Special Functions
            if(splitPath[0] == "ToDateTime")
            {
                DateTime t = new DateTime(Convert.ToInt32(Value) * 1000);
                return t.ToString();
            }

            if (Data == null)
                return "";

            if(splitPath.Length == 1)
            {
                // Current Element return KeyValue
                var r = Data.FirstOrDefault(x => x.Key == splitPath.FirstOrDefault());
                if (r != null)
                    return r.Value;
            }
            else
            {
                var searchFor = splitPath.FirstOrDefault();
                if(searchFor.StartsWith("[") && searchFor.EndsWith("]"))
                {
                    // We need to extract by index
                    var index = Convert.ToInt32(searchFor.Substring(1, searchFor.Length - 2));
                    if (index >= Data.Count)
                        return "";
                    var r2 = Data[index];
                    return r2.GetValue(String.Join(".", splitPath.Reverse().Take(splitPath.Length - 1).Reverse()));
                }
                var r = Data.FirstOrDefault(x => x.Key == splitPath.FirstOrDefault());
                if (r == null)
                    return "";
                return r.GetValue(String.Join(".", splitPath.Reverse().Take(splitPath.Length-1).Reverse()));
            }
            return "";
        }

        public DataMessage()
        {
            Data = new List<DataMessage>();
        }

        public DataMessage Clone()
        {
            var js = this.Serialize();
            return Deserialize(js);
        }

        public string JSONMessage
        {
            get { return Serialize(); }
        }

        #region Serialization

        public string Serialize()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DataMessage));
            var ms = new MemoryStream();
            serializer.WriteObject(ms, this);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static DataMessage Deserialize(string json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DataMessage));
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream ms = new MemoryStream(byteArray);
            var entry = serializer.ReadObject(ms) as DataMessage;
            ms.Close();

            return entry;
        }

        #endregion Serialization

    }
}
