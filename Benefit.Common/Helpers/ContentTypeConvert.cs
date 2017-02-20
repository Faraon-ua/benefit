using System.Text;
using System.Web;

namespace Benefit.Common.Helpers
{
    public class ContentTypeConvert
    {
        public static string SerializeToXwwwFormUrlencoded<T>(T obj)
        {
            var sb = new StringBuilder();
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(HttpUtility.UrlEncode(propertyInfo.Name));
                sb.Append('=');
                sb.Append(HttpUtility.UrlEncode(propertyInfo.GetValue(obj, null).ToString()));
            }
            return sb.ToString();
        }
    }
}
