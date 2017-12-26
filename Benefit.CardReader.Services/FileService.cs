using System.IO;
using System.Xml.Serialization;

namespace Benefit.CardReader.Services
{
    public class FileService
    {
        public static void XmlSerialize<T>(string fileName, T obj, bool clear = false)
        {
            var formatter = new XmlSerializer(typeof(T));
            using (var fs = new FileStream(fileName, clear ? FileMode.Create : FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, obj);
            }
        }

        public static T XmlDeserialize<T>(string fileName)
        {
            var formatter = new XmlSerializer(typeof(T));
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                return (T)formatter.Deserialize(fs);
            }
        }
    }
}
