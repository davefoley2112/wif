using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using System;
using System.Text;

namespace ReservationService.Client.Helpers
{
    public class EntitySerializer
    {
        public static T GetObject<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml");
            }
            DataContractSerializer ser = new DataContractSerializer(typeof(T));
            return (T)ser.ReadObject(XmlReader.Create(new StringReader(xml)));
        }

        public static byte[] GetBytes<T>(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            DataContractSerializer ser = new DataContractSerializer(typeof(T));
            using (MemoryStream msSerialized = new MemoryStream())
            {
                ser.WriteObject(msSerialized, obj);
                msSerialized.Flush();
                byte[] byteSerialized = msSerialized.ToArray();
                return byteSerialized;
            }
        }

        public static string GetString<T>(T obj)
        {
            byte[] array = GetBytes<T>(obj);
            return Encoding.UTF8.GetString(array, 0, array.Length);
        }
    }
}