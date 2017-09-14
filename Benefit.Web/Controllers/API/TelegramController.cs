using System.Threading.Tasks;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Services.Domain;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Benefit.Web.Controllers.API
{
    public class TelegramController : Controller
    {
        private TelegramBotClient _bot = new TelegramBotClient(SettingsService.Telegram.BotToken);
        private SellerService SellerService = new SellerService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [HttpPost]
        public async Task<ActionResult> Webhook(Update update)
        {
            _logger.Info(update);
            var message = update.Message;
            if (message.Type.Equals(MessageType.TextMessage))
            {
                if (SellerService.AddNotificationChannel(message.Text, message.Chat.Id.ToString(),
                    NotificationChannelType.Telegram))
                {
                    await _bot.SendTextMessageAsync(message.Chat.Id, "Telegram сповіщення успішно увімкненно");
                }
                else
                {
                    await _bot.SendTextMessageAsync(message.Chat.Id, "Telegram сповіщення не увімкненно, перевірте чи вірний ідинтифікатор постачальника");
                }
            }
            return new HttpStatusCodeResult(200);
        }
    }
}