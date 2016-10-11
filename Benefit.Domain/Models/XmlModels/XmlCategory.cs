using System.Xml.Linq;

namespace Benefit.Domain.Models.XmlModels
{
    public class XmlCategory
    {
        public XmlCategory(XElement xmlCategory)
        {
            Id = xmlCategory.Element("Ид").Value;
            Name = xmlCategory.Element("Наименование").Value;
            ParentId = xmlCategory.Element("Родитель") == null ? null : xmlCategory.Element("Родитель").Value;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
    }
}
