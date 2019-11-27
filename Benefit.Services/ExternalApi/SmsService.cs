namespace Benefit.Services.ExternalApi
{
    public class SmsService
    {
        public const string npTtnSmsFormat = "Vash zakaz otpravlen! Vi mojete zabrat’ ego na Nova Poshta {0}, TTH: {1}, Stoimost’ {2}grn plus dostavka";
        public const string SmsApiUrl = "https://smsc.ua/sys/send.php?login={0}&psw={1}&phones={2}&mes={3}&sender={4}";

        public void Send(string phone, string message)
        {
            var httpService = new HttpClientService();
            var url = string.Format(SmsApiUrl, SettingsService.SmsApi.Login, SettingsService.SmsApi.Password, phone, message, SettingsService.SmsApi.Name);
            httpService.GetObectFromService<string>(url).ConfigureAwait(false);
        }
    }
}
