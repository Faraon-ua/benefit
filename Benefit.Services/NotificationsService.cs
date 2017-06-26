using System.Threading.Tasks;
using Benefit.Domain.DataAccess;
using Benefit.Services.ExternalApi;

namespace Benefit.Services
{
    public class NotificationsService
    {
        public async Task NotifySeller(int orderNumber, string orderUrl, string sellerId)
        {
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.Find(sellerId);
                if (seller != null && seller.FacebookId != null)
                {
                    var fbService = new FacebookService();
                    var message =
                        string.Format(
                            "Доброго дня. Ви отримали замовлення №{0} на сайті benefit-company.com. Щоб обробити його перейдіть за посиланням {1}.",
                            orderNumber, orderUrl);
                    fbService.SendMessage(seller.FacebookId, message);
                }
            }
        }
    }
}
