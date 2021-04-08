using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using Benefit.DataTransfer.ApiDto.Facebook;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Newtonsoft.Json;

namespace Benefit.Services.ExternalApi
{
    public class FacebookService
    {
        private readonly HttpClientService _clientService = new HttpClientService();

        public void ReceiveMessage(string msg, string fbmsgId)
        {
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.Include(entry => entry.NotificationChannels).FirstOrDefault(entry => entry.Id == msg);
                if (seller != null)
                {
                    if (!seller.NotificationChannels.Any(
                        entry => entry.ChannelType == NotificationChannelType.Facebook && entry.Address == fbmsgId))
                    {
                        var notificationChannel = new NotificationChannel()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ChannelType = NotificationChannelType.Facebook,
                            Address = fbmsgId,
                            SellerId = seller.Id
                        };
                        db.NotificationChannels.Add(notificationChannel);
                        db.SaveChanges();

                    }
                    SendMessage(fbmsgId,
                        string.Format(
                            "Ваш фейсбук акаунт був успішно підв’язаний до автоматичних сповіщень про нові замовлення зроблені у постачальника {0} на сайті benefit.ua",
                            seller.Name));
                }
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

            _clientService.PostObectToService<MessagePostbackResponseDto>(url.ToString(), stringContent);
        }
    }
}
