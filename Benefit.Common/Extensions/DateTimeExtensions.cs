using System;

namespace Benefit.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToDateTimeWithFormat(this DateTime dateTime)
        {
            return dateTime.ToString("d.M.yyyy HH:mm");
        }
        public static string ToLocalDateTimeWithFormat(this DateTime dateTime)
        {
            return dateTime.ToLocalTime().ToString("d.M.yyyy HH:mm");
        }
        public static string ToLocalTimeWithFormat(this DateTime dateTime)
        {
            return dateTime.ToLocalTime().ToString("HH:mm");
        }
        public static string ToLocalTimeWithDateFormat(this DateTime dateTime)
        {
            return dateTime.ToLocalTime().ToString("d.MM.yyyy");
        }

        public static DateTime StartOfDay(this DateTime theDate)
        {
            return theDate.Date;
        }

        public static DateTime EndOfDay(this DateTime theDate)
        {
            return theDate.Date.AddDays(1).AddTicks(-1);
        }

        public static DateTime StartOfMonth(this DateTime theDate)
        {
            return new DateTime(theDate.Year, theDate.Month, 1);
        }

        public static DateTime EndOfMonth(this DateTime theDate)
        {
            return theDate.StartOfMonth().AddMonths(1).AddTicks(-1);
        }
    }
}
