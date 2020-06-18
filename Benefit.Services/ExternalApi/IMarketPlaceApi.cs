using Benefit.Domain.Models;

namespace Benefit.Services.ExternalApi
{
    public interface IMarketPlaceApi
    {
        string GetAccessToken(string userName, string password);
        void UpdateOrderStatus(string id, OrderStatus oldStatus, OrderStatus newStatus, string ttn, int tryCount = 1);
        void ProcessOrders(string getOrdersUrl = null, string authToken = null, int type = 1);
        void ProcessOrders();
    }
}
