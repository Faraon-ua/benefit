using System.ComponentModel;

namespace Benefit.DataTransfer.ApiDto.OldApi
{
    public enum SellerAuthResult
    {
        [Description("success")]
        Success,
        [Description("error login")]
        Error

    }
    public class SellerAuthIngest
    {
        public string username { get; set; }
        public string password { get; set; }       
    }

    public class SellerAuthDto
    {
        public string result { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public short number_check { get; set; }
        public string card { get; set; }
        public float paybals { get; set; }
        public float paybonus { get; set; }
        public float bonus { get; set; }
        public string typeuser { get; set; }
    }
}
