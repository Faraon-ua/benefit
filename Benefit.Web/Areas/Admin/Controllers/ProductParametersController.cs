using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class ProductParametersController : AdminController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Admin/ProductParameters/
        public ActionResult Index(string categoryId)
        {
            if (categoryId == null) return HttpNotFound();
            var category = db.Categories.Find(categoryId);
            var productparameters = db.ProductParameters.Include(p => p.Category).Include(p => p.ParentProductParameter).Where(entry => entry.ParentProductParameterId == null && entry.CategoryId == categoryId);
            var model = new KeyValuePair<Category, IEnumerable<ProductParameter>>(category, productparameters);
            return View(model);
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

        public ActionResult ProductParameterCategory(string id = null, string categoryId = null)
        {
            var productParameter = db.ProductParameters.Find(id) ?? new ProductParameter() { CategoryId = categoryId };
            return PartialView("_ProductParameterGroup", productParameter);
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
        public ActionResult CreateOrUpdate(ProductParameter productparameter)
        {
            if (ModelState.IsValid)
            {
                if (productparameter.Id == null)
                {
                    productparameter.Id = Guid.NewGuid().ToString();
                    productparameter.AddedById = User.Identity.GetUserId();
                    db.ProductParameters.Add(productparameter);
                }
                else
                {
                    db.Entry(productparameter).State = EntityState.Modified;
                }
                db.SaveChanges();
                return RedirectToAction("Index", new { categoryId = productparameter.CategoryId });
            }
            ViewBag.Type = new List<SelectListItem>()
            {
                new SelectListItem() {Text = "Текст", Value = typeof (string).ToString()},
                new SelectListItem() {Text = "Ціле число", Value = typeof (int).ToString()},
                new SelectListItem() {Text = "Десятичне число", Value = typeof (double).ToString()}
            };
            return View(productparameter);
        }

        [HttpPost]
        public ActionResult CreateOrUpdateValue(ProductParameterValue productParameterValue, string categoryId)
        {
            if (ModelState.IsValid)
            {
                productParameterValue.Id = Guid.NewGuid().ToString();
                db.ProductParameterValues.Add(productParameterValue);
                db.SaveChanges();
                return RedirectToAction("Index", new { categoryId });
            }
            return View();
        }

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
