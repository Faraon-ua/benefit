﻿using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Benefit.Domain.Models.Enums;

namespace Benefit.Services
{
    public class SettingsService
    {
        public static string BaseHostName
        {
            get
            {
                return ConfigurationManager.AppSettings["BaseHostName"];
            }
        } 
        public static List<string> SupportedLocalizations
        {
            get
            {
                return ConfigurationManager.AppSettings["SupportedLocalizations"].Split(',').ToList();
            }
        }

        public static int MinUserExternalNumber
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["MinUserExternalNumber"]);
            }
        }
        public static int BonusesComissionRate
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["BonusesComissionRate"]);
            }
        }
        public static int SkuMinValue
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["SkuMinValue"]);
            }
        }

        public static int OrderMinValue
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["OrderMinValue"]);
            }
        }

        public class SendPulse
        {
            public static string SendPulseAuthUrl
            {
                get
                {
                    return ConfigurationManager.AppSettings["SendPulseAuthUrl"];
                }
            }
            public static string SendPulseAddEmailUrl
            {
                get
                {
                    return ConfigurationManager.AppSettings["SendPulseAddEmailUrl"];
                }
            }
            public static string SendPulseFinLikbezAddressBookId
            {
                get
                {
                    return ConfigurationManager.AppSettings["SendPulseFinLikbezAddressBookId"];
                }
            }
        }

        public class Email
        {
            public static string From
            {
                get
                {
                    return ConfigurationManager.AppSettings["EmailFrom"];
                }
            }
            public static string Admin
            {
                get
                {
                    return ConfigurationManager.AppSettings["AdminEmail"];
                }
            }
        }

        public class Images
        {
            public static int UserAvatarMaxWidth
            {
                get
                {
                    return int.Parse(ConfigurationManager.AppSettings["UserAvatarMaxWidth"]);
                }
            }
            public static int UserAvatarMaxHeight
            {
                get
                {
                    return int.Parse(ConfigurationManager.AppSettings["UserAvatarMaxHeight"]);
                }
            }
            public static int SellerGalleryImageMaxWidth
            {
                get
                {
                    return int.Parse(ConfigurationManager.AppSettings["SellerGalleryImageMaxWidth"]);
                }
            }
            public static int SellerGalleryImageMaxHeight
            {
                get
                {
                    return int.Parse(ConfigurationManager.AppSettings["SellerGalleryImageMaxHeight"]);
                }
            }
            public static int SellerLogoMaxWidth
            {
                get
                {
                    return int.Parse(ConfigurationManager.AppSettings["SellerLogoMaxWidth"]);
                }
            }
            public static int SellerLogoMaxHeight
            {
                get
                {
                    return int.Parse(ConfigurationManager.AppSettings["SellerLogoMaxHeight"]);
                }
            }
            public static int NewsLogoMaxWidth
            {
                get
                {
                    return int.Parse(ConfigurationManager.AppSettings["NewsLogoMaxWidth"]);
                }
            }
            public static int NewsLogoMaxHeight
            {
                get
                {
                    return int.Parse(ConfigurationManager.AppSettings["NewsLogoMaxHeight"]);
                }
            }
        }

        public class RewardsPlan
        {
            public static int PointsQualificationAmount = 500;
            public static int VIPPointsQualificationAmount = 1000;
            public static int VIPQualifiedPartnersNumber = 5;
            public static int VIPPortionPointsAmount = 1000;
            public static int VIPMaxPortions = 1000;
            public static int ProfitDay = 15;

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
        }

        public static Dictionary<int, double> DiscountPercentToPointRatio
        {
            get
            {
                return new Dictionary<int, double>()
                {
                    {0, 0.5},
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
                    {20, 2},
                    {21, 2},
                    {22, 2},
                    {23, 2},
                    {24, 2},
                    {25, 2},
                    {26, 2},
                    {27, 2},
                    {28, 2},
                    {29, 2},
                    {30, 2},
                    {999, 0}
                };
            }
        }
    }
}
