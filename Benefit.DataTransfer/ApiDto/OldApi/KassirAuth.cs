namespace Benefit.DataTransfer.ApiDto.OldApi
{
    public class PartnerAuthIngest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string cardId { get; set; }
        public string cardKassir { get; set; }
    }

    public class PartnerAuthDto
    {
        public PartnerAuthDto()
        {
            result = string.Empty;
            cardKassir = string.Empty;
            fioKassir = string.Empty;
            cardClient = string.Empty;
            nameClient = string.Empty;
        }
        public string result { get; set; }
        public string cardKassir { get; set; }
        public string fioKassir { get; set; }
        public string cardClient { get; set; }
        public string nameClient { get; set; }
    }
}
