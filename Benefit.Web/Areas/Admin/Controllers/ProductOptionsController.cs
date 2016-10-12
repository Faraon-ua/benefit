using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class ProductOptionsController : AdminController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Admin/ProductParameters/
        public ActionResult Index(string categoryId = null, string sellerId = null, string productId = null)
        {
            if (User.IsInRole(DomainConstants.SellerRoleName))
            {
                sellerId = Session[DomainConstants.SellerSessionIdKey].ToString();
            }
            var productOptions =
                db.ProductOptions.Where(
                    entry =>
                        entry.CategoryId == categoryId && entry.SellerId == sellerId && entry.ProductId == productId && entry.ParentProductOptionId == null).ToList();

            return View(new ProductOptionsViewModel
            {
                ProductId = productId,
                CategoryId = categoryId,
                SellerId = sellerId,
                ProductOptions = productOptions
            });
        }

        public ActionResult ProductParameterCategory(string id = null, string categoryId = null, string sellerId = null, string productId = null)
        {
            var productParameter = db.ProductOptions.Find(id) ?? new ProductOption() { CategoryId = categoryId, SellerId = sellerId, ProductId = productId };
            return PartialView("_ProductOptionGroup", productParameter);
        }

        public ActionResult GetProductParameterDefinedValues(string parameterId, int? amount = null, string selected = null)
        {
            var parameter = db.ProductParameters.Find(parameterId);
            var values =
                parameter.ProductParameterValues.Select(
                    entry =>
                        new SelectListItem()
                        {
                            Text = entry.ParameterValue,
                            Selected = entry.ParameterValue == selected
                        });
            var model = new ProductParameterProduct()
            {
                Amount = amount,
                ProductParameterId = parameterId
            };
            ViewBag.StartValue = values;
            return PartialView("_ProductOption", model);
        }

       
        public ActionResult ProductParameterValue(string parameterId, string categoryId)
        {
            var productParameter = new ProductParameterValue() { ProductParameterId = parameterId };
            ViewBag.CategoryId = categoryId;
            return PartialView("_ProductParameterValue", productParameter);
        }

        public ActionResult CreateOrUpdate(string categoryId, string parentId = null)
        {
            ViewBag.Type = new List<SelectListItem>()
            {
                new SelectListItem() {Text = "Текст", Value = typeof (string).ToString()},
                new SelectListItem() {Text = "Ціле число", Value = typeof (int).ToString()},
                new SelectListItem() {Text = "Десятичне число", Value = typeof (double).ToString()}
            };
            var productParameter = new ProductParameter()
            {
                CategoryId = categoryId,
                ParentProductParameterId = parentId
            };
            return View(productParameter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrUpdate(ProductOption productparameter)
        {
            if (ModelState.IsValid)
            {
                if (productparameter.Id == null)
                {
                    productparameter.Id = Guid.NewGuid().ToString();
                    db.ProductOptions.Add(productparameter);
                }
                else
                {
                    db.Entry(productparameter).State = EntityState.Modified;
                }
                db.SaveChanges();
                return RedirectToAction("Index", new { categoryId = productparameter.CategoryId, sellerId = productparameter.SellerId, productId = productparameter.ProductId });
            }
            return View(productparameter);
        }

       /* [HttpPost]
        public ActionResult CreateOrUpdateValue(ProductParameterValue productParameterValue, string categoryId)
        {
            throw new Exception();
            if (ModelState.IsValid)
            {
                productParameterValue.Id = Guid.NewGuid().ToString();
                db.ProductParameterValues.Add(productParameterValue);
                db.SaveChanges();
                return RedirectToAction("Index", new { categoryId });
            }
            return View();
        }*/

        public ActionResult Delete(string id, string categoryId)
        {
            ProductParameter productparameter = db.ProductParameters.Find(id);

            db.ProductParameterValues.RemoveRange(productparameter.ProductParameterValues);
            db.ProductParameters.Remove(productparameter);
            db.SaveChanges();
            return RedirectToAction("Index", new { categoryId });
        }

        public ActionResult DeleteValue(string id, string categoryId)
        {
            var productparameterValue = db.ProductParameterValues.Find(id);
            db.ProductParameterValues.Remove(productparameterValue);
            db.SaveChanges();
            return RedirectToAction("Index", new { categoryId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
