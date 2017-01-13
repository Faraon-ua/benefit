using System;

namespace Benefit.Common.Extensions
{
    public static class NumbersExtension
    {
        public static string ToDoubleDigits(this double value)
        {
            return string.Format("{0:0.00}", Math.Truncate(value*100)/100);
        }
    }
}
