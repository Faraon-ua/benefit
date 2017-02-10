namespace Benefit.CardReader.DataTransfer.Ingest
{
    public class TokenIngest
    {
        public string grant_type
        {
            get
            {
                return "password";
            }
        }
        public string username { get; set; }
    }
}
