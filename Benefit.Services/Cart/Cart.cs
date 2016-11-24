using System.Data.Entity;
using System.Linq;
using System.Web;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Services.Cart
{
    public class Cart
    {
        private string SessionKey;
        public Order Order { get; set; }
        public static Cart CurrentInstance
        {
            get
            {
                var sessionKey = string.Format("{0}-{1}", DomainConstants.OrderPrefixKey, HttpContext.Current.Session.SessionID);
                var cart = HttpContext.Current.Session[sessionKey] as Cart ?? new Cart()
                {
                    SessionKey = sessionKey,
                    Order = new Order()
                };
                HttpContext.Current.Session.Add(sessionKey, cart);
                return cart;
            }
        }

        public int AddProduct(OrderProduct orderProduct, string sellerId)
        {
            if (Order.SellerId != null && Order.SellerId != sellerId)
            {
                Order.OrderProducts.Clear();
                Order.OrderProductOptions.Clear();
            }
            using (var db = new ApplicationDbContext())
            {
                var product = db.Products.Include(entry => entry.Currency).FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                orderProduct.ProductName = product.Name;
                orderProduct.ProductPrice = (double)(product.Price * product.Currency.Rate);
                foreach (var orderProductOption in orderProduct.OrderProductOptions)
                {
                    var productOption = db.ProductOptions.Find(orderProductOption.ProductOptionId);
                    orderProductOption.ProductOptionName = productOption.Name;
                    orderProductOption.ProductOptionPriceGrowth = productOption.PriceGrowth;
                }
            }
            Order.SellerId = sellerId;
            Order.OrderProducts.Add(orderProduct);
            HttpContext.Current.Session.Add(SessionKey, this);
            return Order.OrderProducts.Count;
        }

        public int RemoveProduct(string id)
        {
            var productToRemove = Order.OrderProducts.FirstOrDefault(entry => entry.ProductId == id);
            Order.OrderProducts.Remove(productToRemove);
            HttpContext.Current.Session.Add(SessionKey, this);
            return Order.OrderProducts.Count;
        }

        public void Clear()
        {
            Order = new Order();
        }
    }
}
