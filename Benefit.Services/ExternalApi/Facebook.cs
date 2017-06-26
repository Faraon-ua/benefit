using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using Benefit.DataTransfer.ApiDto.Facebook;
using Benefit.Domain.DataAccess;
using Newtonsoft.Json;

namespace Benefit.Services.ExternalApi
{
    public class FacebookService
    {
        private readonly HttpClientService _clientService = new HttpClientService();
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public void ReceiveMessage(string msg, string fbmsgId)
        {
            var seller = db.Sellers.FirstOrDefault(entry => entry.Id == msg);
            if (seller != null)
            {
                seller.FacebookId = fbmsgId;
                db.Entry(seller).State = EntityState.Modified;
                db.SaveChanges();
                SendMessage(fbmsgId, string.Format("Ваш фейсбук акаунт був успішно підв’язаний до автоматичних сповіщень про нові замовлення зроблені у постачальника {0} на сайті benefit-company.com", seller.Name));
            }
        }

        public void SendMessage(string recipientId, string messageText)
        {
            var url = new UriBuilder(SettingsService.Facebook.MessengerAPIUrl);
            var query = HttpUtility.ParseQueryString(url.Query);
            query["access_token"] = SettingsService.Facebook.PageAccessToken;
            url.Query = query.ToString();

            var messageData = new
            {
                recipient = new
                {
                    id = recipientId
                },
                message = new
                {
                    text = messageText
                }
            };
            var json = JsonConvert.SerializeObject(messageData);
            var stringContent = new StringContent(json,
                UnicodeEncoding.UTF8,
                "application/json");

            var response = _clientService.PostObectToService<MessagePostbackResponseDto>(url.ToString(), stringContent);
        }
    }
}
