using System.ComponentModel;

namespace Benefit.Domain.Models.Enums
{
    public enum SellerSortOption
    {
        Rating,
        NameAsc,
        NameDesc,
        BonusAsc,
        BonusDesc
    }

    public enum ProductSortOption
    {
        [Description("Порядковий номер")]
        Order,
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

    public enum OrderSortOption
    {
        [Description("Час &#8595;")]
        DateDesc,
        [Description("Час &#8593;")]
        DateAsc,
        [Description("Сума &#8595;")]
        SumDesc,
        [Description("Сума &#8593;")]
        SumAsc
    }
}
