using Benefit.Domain.Models;
using System.Threading.Tasks;

namespace Benefit.Services.ExternalApi
{
    public interface IMarketPlaceApi
    {
        string GetAccessToken(string userName, string password);
        void UpdateOrderStatus(string id, OrderStatus oldStatus, OrderStatus newStatus, string ttn, int tryCount, string sellerComment);
        Task ProcessOrders(string getOrdersUrl = null, string authToken = null, int type = 1);
    }
}
