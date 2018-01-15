using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Benefit.CardReader.Services
{
    public class FileService
    {
        public static void XmlSerialize<T>(string fileName, T obj, bool clear = false)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var xmlnsEmpty = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName(string.Empty, string.Empty),
            });
            var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, obj, xmlnsEmpty);
            File.WriteAllText(fileName, stringWriter.ToString());
        }

        public static T XmlDeserialize<T>(string fileName)
        {
            var content = File.ReadAllText(fileName);
            var xmlSerializer = new XmlSerializer(typeof(T));
            var stringReader = new StringReader(content);
            return (T)xmlSerializer.Deserialize(stringReader);

            var formatter = new XmlSerializer(typeof(T));
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                if (fs.Length == 0) return default(T);
                return (T)formatter.Deserialize(fs);
            }
        }
    }
}
