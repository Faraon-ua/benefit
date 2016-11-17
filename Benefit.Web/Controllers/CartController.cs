using System.Web.Mvc;
using System.Linq;
using System.Data.Entity;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Cart;

namespace Benefit.Web.Controllers
{
    public class CartController : Controller
    {
        public ActionResult AddProduct(OrderProduct product)
        {
            var productsNumber = Cart.CurrentInstance.AddProduct(product);
            return Json(productsNumber);
        }

        public ActionResult RemoveProduct(string productId)
        {
            var productsNumber = Cart.CurrentInstance.RemoveProduct(productId);
            return Json(productsNumber);
        }

        public ActionResult Index()
        {
            var cart = Cart.CurrentInstance;
            using (var db = new ApplicationDbContext())
            {
                foreach (var orderProduct in cart.Order.OrderProducts)
                {
                    var product = db.Products.Include(entry=>entry.Currency).FirstOrDefault(entry=>entry.Id == orderProduct.ProductId);
                    orderProduct.ProductName = product.Name;
                    orderProduct.ProductPrice = (double) (product.Price * product.Currency.Rate);
                    foreach (var orderProductOption in orderProduct.OrderProductOptions)
                    {
                        var productOption = db.ProductOptions.Find(orderProductOption.ProductOptionId);
                        orderProductOption.ProductOptionName = productOption.Name;
                        orderProductOption.ProductOptionPriceGrowth= productOption.PriceGrowth;
                    }
                }
            }

            return View(cart.Order);
        }
	}
}