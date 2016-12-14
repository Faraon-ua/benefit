using System.Linq;
using System.Xml.Linq;

namespace Benefit.Domain.Models.XmlModels
{
    public class XmlProduct
    {
        public XmlProduct(XElement xmlProduct)
        {
            Id = xmlProduct.Element("Ид").Value;
            Name = xmlProduct.Element("Наименование").Value;
            Description = xmlProduct.Element("Описание") == null ? string.Empty : xmlProduct.Element("Описание").Value;
            CategoryId = xmlProduct.Element("Группы").Element("Ид").Value;
            Image = xmlProduct.Element("Картинка").Value;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryId { get; set; }
        public string Image { get; set; }
    }
}
