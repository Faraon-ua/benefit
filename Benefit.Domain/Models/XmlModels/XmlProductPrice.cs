using System.Globalization;
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

            double amountValue = 0;
            Id = xmlProduct.Element("Ид").Value;
            Price = double.Parse(priceValue.Value, CultureInfo.InvariantCulture);
            double.TryParse(xmlProduct.Element("Ид").Value, out amountValue);
            Amount = amountValue;
        }

        public string Id { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
    }
}
