using Benefit.Common.Constants;

namespace Benefit.Common.Extensions
{
    public static class UrlExtensions
    {
        public static string AddSortToUrl(this string url, string filterValue)
        {
            if (url.Contains(FilterAndSortingConstants.SortUrlName))
            {
                var startIndex = url.IndexOf(FilterAndSortingConstants.SortUrlName) +
                                 FilterAndSortingConstants.SortUrlName.Length + 1;
                var endIndex = url.IndexOf("&", startIndex);
                endIndex = endIndex == -1 ? url.Length : endIndex;
                var existingFiterValue = url.Substring(startIndex, endIndex - startIndex);
                return url.Replace(existingFiterValue, filterValue);
            }
            return string.Format("{0}&{1}={2}", url, FilterAndSortingConstants.SortUrlName, filterValue);
        }
    }
}
