using System.ComponentModel;

namespace Benefit.Domain.Models.Enums
{
    public enum BusinessLevel
    {
        [Description("Максимальний")]
        Ultimate,
        [Description("Преміум")]
        Premium,
        [Description("Стандарт")]
        Standart,
        [Description("Клієнт")]
        Client
    }
}
