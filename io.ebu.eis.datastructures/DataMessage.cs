using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Permissions;
using System.Text;

namespace io.ebu.eis.datastructures
{
    /// <summary>
    /// Matches the DataMessage case class in the DataGateway project
    /// https://github.com/ebu/DataGateway
    /// </summary>
    [DataContract]
    public class DataMessage
    {
        private object _lockSync;

        
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
            if (path.Contains(";"))
            {
                var valSplit = path.Split(';');
                var result = "";
                var first = true;
                var i = 0;
                while (i < valSplit.Length)
                {
                    if (!first)
                        result += " - ";
                    first = false;
                    result += GetValue(valSplit[i]);
                    ++i;
                }
                return result;
            }
            else
            {

                var splitPath = path.Split('.');

                // TODO Enhance
                // Evaluate Special Functions
                if (splitPath[0] == "ToDateTime")
                {
                    DateTime t = new DateTime(Convert.ToInt32(Value) * 1000);
                    return t.ToString(CultureInfo.InvariantCulture);
                }

                if (Data == null)
                    return "";

                // TODO is is weird and should not be necessary
                if (_lockSync == null)
                    _lockSync = new object();

                lock (_lockSync)
                {
                    if (splitPath.Length == 1)
                    {
                        // Current Element return KeyValue
                        var r = Data.FirstOrDefault(x => x.Key == splitPath.FirstOrDefault());
                        if (r != null)
                            return r.Value;

                    }
                    else
                    {
                        var searchFor = splitPath.FirstOrDefault();
                        if (searchFor != null && (searchFor.StartsWith("[") && searchFor.EndsWith("]")))
                        {
                            // We need to extract by index
                            var index = Convert.ToInt32(searchFor.Substring(1, searchFor.Length - 2));
                            if (index < 0)
                                index = Data.Count + index;
                            if (index >= Data.Count)
                                return "";
                            var r2 = Data[index];
                            return
                                r2.GetValue(String.Join(".", splitPath.Reverse().Take(splitPath.Length - 1).Reverse()));
                        }
                        else if (searchFor != null && (searchFor.StartsWith("(") && searchFor.EndsWith(")")))
                        {
                            // We need to extract by DataType
                            var r2 = Data.FirstOrDefault(x => x.DataType == searchFor.Substring(1, searchFor.Length - 2));
                            if (r2 != null)
                                return
                                    r2.GetValue(String.Join(".",
                                        splitPath.Reverse().Take(splitPath.Length - 1).Reverse()));
                        }
                        var r = Data.FirstOrDefault(x => x.Key == splitPath.FirstOrDefault());
                        if (r == null)
                            return "";
                        return r.GetValue(String.Join(".", splitPath.Reverse().Take(splitPath.Length - 1).Reverse()));
                    }
                }
                return "";
            }
        }

        public DataMessage()
        {
            _lockSync = new object();
            lock (_lockSync)
            {
                Data = new List<DataMessage>();
            }
        }

        public DataMessage Clone()
        {
            var js = Serialize();
            return Deserialize(js);
        }

        /// <summary>
        /// Merges a global Data to the COntext
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Returns true if changes happened</returns>
        public bool MergeGlobal(DataMessage message)
        {
            var changes = false;
            lock (_lockSync)
            {
                foreach (var dm in message.Data)
                {
                    // Remove existing Data Key
                    if (Data.Any(x => x.Key == dm.Key))
                    {
                        var d = Data.FirstOrDefault(x => x.Key == dm.Key);
                        // If content is different the change it
                        if (String.Compare(d.JSONMessage, dm.JSONMessage, StringComparison.Ordinal) != 0)
                        {
                            d.Data = dm.Clone().Data;
                            d.Value = dm.Value;
                            changes = true;
                        }
                    }
                    else
                    {
                        // Add the data if not existing
                        Data.Add(dm.Clone());
                        changes = true;
                    }
                }
            }
            return changes;
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
            entry._lockSync = new object();
            return entry;
        }

        #endregion Serialization

    }
}
