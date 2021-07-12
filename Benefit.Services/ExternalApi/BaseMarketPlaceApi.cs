using Benefit.Domain.Models;
using System.Threading.Tasks;

namespace Benefit.Services.ExternalApi
{
    public enum MarketplaceType
    {
        Rozetka,
        Allo
    }
    public abstract class BaseMarketPlaceApi
    {
        public const int limit = 50;
        public abstract string GetAccessToken(string userName, string password);
        public abstract void UpdateOrderStatus(string id, OrderStatus oldStatus, OrderStatus newStatus, string ttn, int tryCount = 1, string sellerComment = null);
        public abstract Task ProcessOrders(string getOrdersUrl = null, string authToken = null, int type = 1, int offset = 0);
        public static BaseMarketPlaceApi GetMarketplaceServiceInstance(MarketplaceType type)
        {
            switch (type)
            {
                case MarketplaceType.Rozetka:
                    return new RozetkaApiService();
                case MarketplaceType.Allo:
                    return new AlloApiService();
            }
            return null;
        }
        public static BaseMarketPlaceApi GetMarketplaceServiceInstance(OrderType type)
        {
            switch (type)
            {
                case OrderType.Rozetka:
                    return new RozetkaApiService();
                case OrderType.Allo:
                    return new AlloApiService();
            }
            return null;
        }
    }
}
