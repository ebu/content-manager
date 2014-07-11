//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Serialization;

//namespace io.ebu.eis.mq
//{
//    [XmlRoot(ElementName = "OdfBody")]
//    public class OdfBody
//    {
//        [XmlAttribute(AttributeName = "DocumentCode")]
//        public string DocumentCode { get; set; }
//        [XmlAttribute(AttributeName = "DocumentType")]
//        public string DocumentType { get; set; }
//        [XmlAttribute(AttributeName = "Venue")]
//        public string Venue { get; set; }
//        [XmlAttribute(AttributeName = "Date")]
//        public string Date { get; set; }
//        [XmlAttribute(AttributeName = "Time")]
//        public string Time { get; set; }
//        [XmlAttribute(AttributeName = "LogicalDate")]
//        public string LogicalDate { get; set; }
//        [XmlAttribute(AttributeName = "FeedFlag")]
//        public string FeedFlag { get; set; }
//        [XmlAttribute(AttributeName = "DocumentSubcode")]
//        public string DocumentSubcode { get; set; }
//        [XmlAttribute(AttributeName = "Version")]
//        public string Version { get; set; }
//        [XmlAttribute(AttributeName = "Serial")]
//        public string Serial { get; set; }

//        [XmlElement(ElementName = "Competition")]
//        public Competition Competition { get; set; }
//    }

//    public class Competition
//    {
//        [XmlAttribute(AttributeName = "Code")]
//        public string Code { get; set; }

//        [XmlElement(ElementName = "ImageData")]
//        public ImageData ImageData { get; set; }
//    }

//    public class ImageData
//    {
//        [XmlText]
//        public string data { get; set; }
//    }
//}
