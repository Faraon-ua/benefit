using System;
using System.Text.RegularExpressions;

namespace Benefit.Domain.Models.ModelExtensions
{
    public static class StringExt
    {
        public static string Sanitize(this string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@-]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5)).Replace("'","");
            }
            // If we timeout when replacing invalid characters, 
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }
    }
}
