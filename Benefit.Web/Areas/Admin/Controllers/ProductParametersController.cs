using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services.Domain;
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
            var productparameters = db.ProductParameters.Include(p => p.Category).Where(entry => entry.CategoryId == categoryId).ToList();
            var model = new KeyValuePair<Category, IEnumerable<ProductParameter>>(category, productparameters);
            return View(model);
        }

        public ActionResult GetProductParameterDefinedValues(string parameterId, int? amount = null, string selectedValue = null, string selectedText = null)
        {
            var parameter = db.ProductParameters.Find(parameterId);
            var values =
                parameter.ProductParameterValues.Select(
                    entry =>
                        new SelectListItem()
                        {
                            Text = entry.ParameterValue,
                            Value = entry.ParameterValueUrl,
                            Selected = entry.ParameterValueUrl == selectedValue
                        });
            var model = new ProductParameterProduct()
            {
                Amount = amount,
                ProductParameterId = parameterId,
                StartText = selectedText ?? values.First().Text
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
                CategoryId = categoryId
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
                    productparameter.AddedBy = User.Identity.Name;
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
            var productService = new ProductsService();
            productService.DeleteProductParameter(id);
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
