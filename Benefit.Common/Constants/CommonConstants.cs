using System.Collections.Generic;

namespace Benefit.Common.Constants
{
    public class CommonConstants
    {
        public static readonly Dictionary<int?, string> RatingToClass = new Dictionary<int?, string>()
                {
                    {0, "none"},
                    {1, "very_bed"},
                    {2, "bed"},
                    {3, "middle"},
                    {4, "good"},
                    {5, "top"}
                };
    }
}
