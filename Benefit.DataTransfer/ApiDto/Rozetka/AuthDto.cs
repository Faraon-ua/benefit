namespace Benefit.DataTransfer.ApiDto.Rozetka
{
    public class AuthDtoContent
    {
        public string id { get; set; }
        public string access_token { get; set; }
    }
    public class AuthDto
    {
        public bool success { get; set; }
        public AuthDtoContent content { get; set; }
    }
}
