using System.Xml.Linq;

namespace Benefit.Common.Extensions
{
    public static class XmlExtensions
    {
        public static string GetValueOrDefault(this XElement xElement, string val)
        {
            return xElement == null ? val : xElement.Value;
        }
    }
}
