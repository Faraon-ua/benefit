using System.Threading.Tasks;
using System.Web.Http;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Services.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Benefit.Web.Controllers.API
{
    public class TelegramController : ApiController
    {
        private TelegramBotClient _bot = new TelegramBotClient(SettingsService.Telegram.BotToken);
        private SellerService SellerService = new SellerService();

        public async Task<IHttpActionResult> Post(Update update)
        {
            var message = update.Message;
            if (message.Type.Equals(MessageType.TextMessage))
            {
                if (message.Text == "/start")
                {
                    await _bot.SendTextMessageAsync(message.Chat.Id, "Доброго дня, ведіть - 1 для сповіщень про замовлення, 2 - для сповіщень про помилки в обробці замовленнь марктеплейсів");
                }
                else if(message.Text == "1")
                {
                    await _bot.SendTextMessageAsync(message.Chat.Id, "Введіть ідентифікатор постачальника");
                }
                else if (message.Text == "2")
                {
                    var result = SellerService.AddNotificationChannel(null, message.Chat.Id.ToString(),
                        NotificationChannelType.TelegramApiFail);
                    if (result != null)
                    {
                        await _bot.SendTextMessageAsync(message.Chat.Id, "Telegram сповіщення успішно увімкненно");
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(message.Chat.Id, "Telegram сповіщення не увімкненно");
                    }
                }
                else
                {
                    var result = SellerService.AddNotificationChannel(message.Text, message.Chat.Id.ToString(),
                        NotificationChannelType.Telegram);
                    if (result != null)
                    {
                        await _bot.SendTextMessageAsync(message.Chat.Id, "Telegram сповіщення успішно увімкненно для " + result);
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(message.Chat.Id, "Telegram сповіщення не увімкненно, перевірте чи вірний ідинтифікатор постачальника");
                    }
                }

            }
            return Ok();
        }
    }
}