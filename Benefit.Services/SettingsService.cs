using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Benefit.Domain.Models.Enums;

namespace Benefit.Services
{
    public class SettingsService
    {
        public static List<string> SupportedLocalizations
        {
            get
            {
                return ConfigurationManager.AppSettings["SupportedLocalizations"].Split(',').ToList();
            }
        }

        public static int SkuMinValue
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["SkuMinValue"]);
            }
        }

        public static double[] UserDiscounts = { 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        public static Dictionary<DistributionType, int> DistributionToPointsPercentageMap
        {
            get
            {
                return new Dictionary<DistributionType, int>()
                {
                    {DistributionType.Mentor, 4},
                    {DistributionType.VIP, 2},
                    {DistributionType.Director, 2},
                    {DistributionType.Silver, 2},
                    {DistributionType.Gold, 1},
                    {DistributionType.SellerInvolvement, 1}
                };
            }
        }

        public static Dictionary<int, int> DiscountPercentToPointRatio
        {
            get
            {
                return new Dictionary<int, int>()
                {
                    {2, 20},
                    {3, 10},
                    {4, 8},
                    {5, 6},
                    {6, 5},
                    {7, 4},
                    {8, 4},
                    {9, 4},
                    {10, 2},
                    {11, 2},
                    {12, 2},
                    {13, 2},
                    {14, 2},
                    {15, 2},
                    {16, 2},
                    {17, 2},
                    {18, 2},
                    {19, 2},
                    {20, 2}
                };
            }
        }
    }
}
