using System.ComponentModel;

namespace Benefit.DataTransfer
{
    public enum ActiveStatus
    {
        [Description("active")]
        Active,
        [Description("inactive")]
        Inactive,
        [Description("")]
        None
    }
}
