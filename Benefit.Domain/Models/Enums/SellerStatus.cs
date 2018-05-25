using System.ComponentModel;

namespace Benefit.Domain.Models.Enums
{
    public enum SellerStatus
    {
        [Description("Не має")]
        None,
        [Description("Стандарт")]
        Standart,
        [Description("Преміум")]
        Premium,
        [Description("Професіональний")]
        Professional,
        [Description("Максимальний")]
        Ultimate
    }
}
