using System.Linq;
using System.Xml.Linq;

namespace Benefit.Domain.Models.XmlModels
{
    public class XmlProduct
    {
        public XmlProduct(XElement xmlProduct)
        {
            var priceValue =
                xmlProduct.Element("ЗначенияСвойств")
                    .Elements("ЗначенияСвойства")
                    .First(entry => entry.Element("Наименование").Value == "Цена")
                    .Element("Значение");

            var availabilityValue = xmlProduct.Element("ЗначенияСвойств")
                    .Elements("ЗначенияСвойства")
                    .First(entry => entry.Element("Наименование").Value == "Наличие")
                    .Element("Значение");
          
            Id = xmlProduct.Element("Ид").Value;
            Name = xmlProduct.Element("Наименование").Value;
            Description = xmlProduct.Element("Описание") == null ? string.Empty : xmlProduct.Element("Описание").Value;
            CategoryId = xmlProduct.Element("Группы").Element("Ид").Value;
            Price = priceValue == null ? null : (double?)double.Parse(priceValue.Value);
            Availability = availabilityValue != null && bool.Parse(availabilityValue.Value);
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryId { get; set; }
        public double? Price { get; set; }
        public bool Availability  { get; set; }
    }
}
