using System.ComponentModel;

namespace Benefit.Domain.Models.Enums
{
    public enum ProductSortOption
    {
        Default,
        [Description("Назва &#8593;")]
        NameAsc,
        [Description("Назва &#8595;")]
        NameDesc,
        [Description("Код товару &#8593;")]
        SKUAsc,
        [Description("Код товару &#8595;")]
        SKUDesc,
        [Description("Ціна &#8593;")]
        PriceAsc,
        [Description("Ціна &#8595;")]
        PriceDesc
    }
}
