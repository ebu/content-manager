using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace io.ebu.eis.datastructures
{
    /// <summary>
    /// Matches the DataMessage case class in the DataGateway project
    /// https://github.com/ebu/DataGateway
    /// </summary>
    [DataContract]
    public class DispatchNotificationMessage
    {
        public DispatchNotificationMessage()
        {
            ImageVariants = new List<ImageVariant>();
        }

        [DataMember(Name = "receiveTime")]
        public long ReceiveTimeLong
        {
            get { return Convert.ToInt64(ReceiveTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds); }
            set { ReceiveTime = new DateTime(1970, 1, 1).AddMilliseconds(value); }
        }

        public DateTime ReceiveTime { get; set; }

        [DataMember(Name = "account")]
        public string Account { get; set; }

        [DataMember(Name = "source")]
        public string Source { get; set; }

        [DataMember(Name = "contentType")]
        public string ContentType { get; set; }

        [DataMember(Name = "notificationKey")]
        public string NotificationKey { get; set; }

        [DataMember(Name = "notificationMessage")]
        public string NotificationMessage { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "link")]
        public string Link { get; set; }

        [DataMember(Name = "imageurl")]
        public string Imageurl { get; set; }

        [DataMember(Name = "imagevariants")]
        public List<ImageVariant> ImageVariants { get; set; }


        #region Serialization

        public string Serialize()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DispatchNotificationMessage));
            var ms = new MemoryStream();
            serializer.WriteObject(ms, this);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static DispatchNotificationMessage Deserialize(string json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DispatchNotificationMessage));
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream ms = new MemoryStream(byteArray);
            var entry = serializer.ReadObject(ms) as DispatchNotificationMessage;
            ms.Close();

            return entry;
        }

        #endregion Serialization

    }
}
