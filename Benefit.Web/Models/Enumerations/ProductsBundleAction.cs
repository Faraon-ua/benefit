using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Benefit.Web.Models.Enumerations
{
    public enum ProductsBulkAction
    {
        [Display(Name = "Задати категорію")]
        SetCategory,
        [Display(Name = "Видалити обрані")]
        DeleteSelected,
        [Display(Name = "Видалити відфільтровані")]
        DeleteAll
    }
}