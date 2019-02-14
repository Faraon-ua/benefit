using System.ComponentModel.DataAnnotations;

namespace Benefit.Web.Models.Enumerations
{
    public enum ProductsBulkAction
    {
        [Display(Name = "Задати категорію")]
        SetCategory,
        [Display(Name = "Видалити обрані")]
        DeleteSelected,
        [Display(Name = "Видалити відфільтровані")]
        DeleteAll,
        [Display(Name="Додати в експорт обрані")]
        ExportSelected,
        [Display(Name="Додати в експорт відсортовані")]
        ExportAll
    }
}