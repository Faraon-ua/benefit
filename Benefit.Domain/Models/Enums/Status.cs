using System.ComponentModel;

namespace Benefit.Domain.Models.Enums
{
    public enum Status
    {
        [Description("PARTNER")]
        Partner,
        [Description("VIP PARTNER")]
        VIP,
        [Description("DIRECTOR")]
        Director,
        [Description("SILVER DIRECTOR")]
        Silver,
        [Description("GOLD DIRECTOR")]
        Gold,
        [Description("DIAMOND DIRECTOR")]
        Diamond
    }
}
