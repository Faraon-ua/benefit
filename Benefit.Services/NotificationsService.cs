using System.Threading.Tasks;
using Benefit.Domain.DataAccess;
using Benefit.Services.ExternalApi;
using System.Data.Entity;
using System.Linq;
using Benefit.Domain.Models;
using Telegram.Bot;

namespace Benefit.Services
{
    public class NotificationsService
    {
        public void NotifySeller(int orderNumber, string orderUrl, string sellerId)
        {
            var message = string.Format(
                "Доброго дня. Ви отримали замовлення №{0} на сайті benefit-company.com. Щоб обробити його перейдіть за посиланням {1}.",
                orderNumber, orderUrl);
            using (var db = new ApplicationDbContext())
            {
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
                                "Доброго дня. Ви отримали замовлення №{0} на сайті benefit-company.com. Щоб обробити його перейдіть за посиланням <a href='{1}'>{1}</a>.",
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
