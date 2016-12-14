using System.Linq;
using System.Xml.Linq;

namespace Benefit.Domain.Models.XmlModels
{
    public class XmlProductPrice
    {
        public XmlProductPrice(XElement xmlProduct)
        {
            var priceValue =
                xmlProduct
                    .Element("Цены")
                    .Element("Цена")
                    .Element("ЦенаЗаЕдиницу");

            Id = xmlProduct.Element("Ид").Value;
            Price = double.Parse(priceValue.Value);
        }

        public string Id { get; set; }
        public double Price { get; set; }
    }
}
