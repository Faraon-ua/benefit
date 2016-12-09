namespace Benefit.DataTransfer.ApiDto.OldApi
{
    public class GetOrdersIngest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string idTerminal { get; set; }
    }

    public class GetOrdersDto
    {
        public int order { get; set; }
    }
}
