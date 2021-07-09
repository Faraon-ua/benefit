using System;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Helpers;
using Benefit.Domain.Models;

namespace Benefit.Web.Helpers
{
    public static class EnumExtensions
    {
        public static SelectList ToSelectList(this Enum enumObj)
        {
            Type t = enumObj.GetType();
            var values = from BannerType e in Enum.GetValues(t)
                         select new SelectListItem { Value = e.ToString(), Text = Enumerations.GetDisplayNameValue(e)};
            return new SelectList(values, "Value", "Text", enumObj);
        }
    }
}