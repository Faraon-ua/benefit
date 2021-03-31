using System.Threading.Tasks;
using Benefit.Domain.DataAccess;
using Benefit.Services.ExternalApi;
using System.Data.Entity;
using System.Linq;
using Benefit.Domain.Models;
using Telegram.Bot;
using Benefit.Common.Helpers;
using System;

namespace Benefit.Services
{
    public class NotificationsService
    {
        public string AddApiFailNotification(string apiName, string channelAddress, NotificationChannelType channelType)
        {
            using (var db = new ApplicationDbContext())
            {
                var existingNotification =
                db.NotificationChannels.FirstOrDefault(
                    entry => entry.Name == apiName && entry.Address == channelAddress);
                if (existingNotification != null)
                {
                    return apiName;
                }
                var notificationCHannel = new NotificationChannel()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = apiName,
                    ChannelType = channelType,
                    Address = channelAddress
                };
                db.NotificationChannels.Add(notificationCHannel);
                db.SaveChanges();
                return apiName;
            }
        }
        public void NotifyApiFailRequest(string orderNumber, string marketPlaceName, OrderStatus oldStatus, OrderStatus newStatus)
        {
            using (var db = new ApplicationDbContext())
            {
                var message = string.Format(
                "Статус замовлення №{0} не було змінено на маркетплейсі {1}, {2} -> {3}",
                orderNumber, marketPlaceName, Enumerations.GetDisplayNameValue(oldStatus), Enumerations.GetDisplayNameValue(newStatus));
                var notificationChannels = db.NotificationChannels.Where(entry => entry.ChannelType == NotificationChannelType.TelegramApiFail && entry.Name == marketPlaceName).ToList();
                foreach (var notificationChannel in notificationChannels)
                {
                    var telegram = new TelegramBotClient(SettingsService.Telegram.BotToken);
                    telegram.SendTextMessageAsync(notificationChannel.Address, message).ConfigureAwait(false);
                }
            }
        }
        public async Task NotifyApiFailRequest(string orderNumber, string marketPlaceName, string message)
        {
            using (var db = new ApplicationDbContext())
            {
                var notificationChannels = db.NotificationChannels.Where(entry => entry.ChannelType == NotificationChannelType.TelegramApiFail && entry.Name == marketPlaceName).ToList();
                foreach (var notificationChannel in notificationChannels)
                {
                    var telegram = new TelegramBotClient(SettingsService.Telegram.BotToken);
                    var result = await telegram.SendTextMessageAsync(notificationChannel.Address, message);
                }
            }
        }
        public void NotifySeller(int orderNumber, string orderUrl, string sellerId)
        {
            using (var db = new ApplicationDbContext())
            {
                var message = string.Format(
                "Доброго дня. Ви отримали замовлення №{0} на сайті benefit.ua. Щоб обробити його перейдіть за посиланням {1}",
                orderNumber, orderUrl);
                var seller = db.Sellers.Include(entry => entry.NotificationChannels).FirstOrDefault(entry => entry.Id == sellerId);
                if (seller != null)
                {
                    foreach (var notificationChannel in seller.NotificationChannels)
                    {
                        //Facebook
                        if (notificationChannel.ChannelType == NotificationChannelType.Facebook)
                        {
                            var fbService = new FacebookService();
                            fbService.SendMessage(notificationChannel.Address, message);
                        }
                        //Telegram
                        if (notificationChannel.ChannelType == NotificationChannelType.Telegram)
                        {
                            var telegram = new TelegramBotClient(SettingsService.Telegram.BotToken);
                            telegram.SendTextMessageAsync(notificationChannel.Address, message);
                        }
                        //SMS
                        if (notificationChannel.ChannelType == NotificationChannelType.Phone)
                        {

                        }
                        //Email
                        if (notificationChannel.ChannelType == NotificationChannelType.Email)
                        {
                            var emailMessage = string.Format(
                                "Доброго дня. Ви отримали замовлення №{0} на сайті benefit.ua. Щоб обробити його перейдіть за посиланням <a href='{1}'>{1}</a>.",
                                orderNumber, orderUrl);
                            var mailService = new EmailService();
                            mailService.SendEmail(notificationChannel.Address, emailMessage, "Нове замовлення на сайті benefit-company.com");
                        }
                    }
                }
            }
        }
    }
}
