using System;

namespace Benefit.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToLocalTimeWithFormat(this DateTime dateTime)
        {
            return dateTime.ToLocalTime().ToString("d.M.yyyy HH:mm");
        }
        public static string ToLocalTimeWithDateFormat(this DateTime dateTime)
        {
            return dateTime.ToLocalTime().ToString("d.M.yyyy");
        }
    }
}
