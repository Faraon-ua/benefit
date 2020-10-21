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
        ExportAll,
        [Display(Name="Видалити із експорту обрані")]
        DeleteFromExport,
        [Display(Name="Видалити із експорту відсортовані")]
        DeleteFromExportAll,
        [Display(Name="Застосувати курс/індекс для обраних")]
        ApplyCurrency,
        [Display(Name = "Застосувати курс/індекс для відсортованих")]
        ApplyCurrencyAll,
        [Display(Name="Модерувати обрані")]
        Moderate,
        [Display(Name = "Модерувати відсортовані")]
        ModerateAll,
        [Display(Name = "Назначити модератора обраним товарам")]
        AssignModerator,
        [Display(Name = "Назначити модератора відсортованим товарам")]
        AssignModeratorAll,
    }
}