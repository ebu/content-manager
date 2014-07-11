using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace SMPAG.MM.MMConnector
{
    internal static class Serializer
    {

        /// <summary>
        /// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
        /// </summary>
        /// <param name="characters">Unicode Byte Array to be converted to String</param>
        /// <returns>String converted from Unicode Byte Array</returns>
        private static string UTF8ByteArrayToString(byte[] characters)
        {
            if (characters == null)
                return null;

            //Encoding utf8 = Encoding.UTF8;
            UTF8Encoding encoding = new UTF8Encoding();
            string constructedString = encoding.GetString(characters);
            //string constructedString = utf8.GetString(characters);
            return (constructedString);
        }

        /// <summary>
        /// Converts the String to UTF8 Byte array and is used in De serialization
        /// </summary>
        /// <param name="pXmlString"></param>
        /// <returns></returns>
        private static Byte[] StringToUTF8ByteArray(string pXmlString)
        {
            if (pXmlString == null)
                return null;
            UTF8Encoding encoding = new UTF8Encoding();
            //TODO Here when string is null cuases an exception !
            byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }

        /// <summary>
        /// Serialize an object into an XML string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static string SerializeObject<T>(T obj)
        {
            try
            {
                string xmlString = null;
                MemoryStream memoryStream = new MemoryStream();

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                XmlSerializer xs = new XmlSerializer(typeof(T));
                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                xmlTextWriter.Formatting = Formatting.Indented;
                xs.Serialize(xmlTextWriter, obj, ns);
                memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
                xmlString = UTF8ByteArrayToString(memoryStream.ToArray()); return xmlString;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Serializes an object into an XML file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file"></param>
        /// <param name="obj"></param>
        internal static void SerializeObject<T>(string file, T obj, Encoding enc)
        {
            string xml = SerializeObject(obj);
            FileStream writer = new FileStream(file, FileMode.Create);
            XmlSerializer xs = new XmlSerializer(typeof(T));
            
            XmlTextWriter xmlTextWriter = new XmlTextWriter(writer, enc);
            xmlTextWriter.Formatting = Formatting.Indented;

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xs.Serialize(xmlTextWriter, obj, ns);
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Reconstruct an object from an XML string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        internal static T DeserializeObject<T>(string xml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(xml));
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            return (T)xs.Deserialize(memoryStream);
        }

        internal static T DeserializeFileObject<T>(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            XmlSerializer xs = new XmlSerializer(typeof(T));
            XmlTextReader reader = new XmlTextReader(fs);
            //XmlTextWriter xmlTextWriter = new XmlTextWriter(reader, Encoding.UTF8);
            T obj = (T)xs.Deserialize(reader);
            reader.Close();
            fs.Close();

            return obj;
        }

    }
}
