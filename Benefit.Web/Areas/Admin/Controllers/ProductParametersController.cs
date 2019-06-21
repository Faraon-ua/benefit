using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services.Domain;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Helpers;
using Benefit.DataTransfer.JSON;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class ProductParametersController : AdminController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Admin/ProductParameters/
        public ActionResult Index(string categoryId)
        {
            if (categoryId == null) return HttpNotFound();
            var category = db.Categories
                .Include(entry=>entry.MappedCategories.Select(mc=>mc.ProductParameters))
                .Include(entry=>entry.MappedCategories.Select(mc=>mc.Seller))
                .FirstOrDefault(entry=>entry.Id == categoryId);
            var productparameters = db.ProductParameters
                .Include(p => p.Category)
                .Include(p => p.MappedProductParameters)
                .Where(entry => entry.CategoryId == categoryId).OrderBy(entry=>entry.Order).ToList();
            ViewBag.Category = category;
            var model = new Dictionary<Seller, IEnumerable<ProductParameter>>
            {
                {new Seller()
                {
                    Name = "Основні"
                }, productparameters}
            };
            var catsBySeller = category.MappedCategories.GroupBy(entry => entry.Seller).ToList();
            foreach (var catBySeller in catsBySeller)
            {
                model.Add(catBySeller.Key, catBySeller.SelectMany(entry=>entry.ProductParameters));
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(List<string> sortedParameters)
        {
            var parameters = db.ProductParameterValues.Where(entry => sortedParameters.Contains(entry.Id)).ToList();
            for (var i = 0; i < sortedParameters.Count; i++)
            {
                var parameter = parameters.FirstOrDefault(entry => entry.Id == sortedParameters[i]);
                parameter.Order = i;
                db.Entry(parameter).State = EntityState.Modified;
            }
            db.SaveChanges();
            return Json("Сортування параметрів збережено");
        }

        [HttpPost]
        public ActionResult ConnectToParameter(string sourceId, string targetId)
        {
            var sourcePP = db.ProductParameters
                .Include(entry => entry.Category.Seller)
                .FirstOrDefault(entry => entry.Id == sourceId);
            sourcePP.ParentProductParameterId = targetId;
            if (!db.MappedProductParameters.Any(entry =>
                entry.ProductParameterId == targetId && entry.SellerId == sourcePP.Category.SellerId &&
                entry.Name == sourcePP.Name))
            {
                var mappedPP = new MappedProductParameter()
                {
                    ProductParameterId = targetId,
                    SellerId = sourcePP.Category.SellerId,
                    Name = sourcePP.Name
                };
                db.MappedProductParameters.Add(mappedPP);
                db.Entry(sourcePP).State = EntityState.Modified;
            }

            db.SaveChanges();
            return new HttpStatusCodeResult(200);
        }

        public ActionResult GetProductParameterDefinedValues(string parameterId, int? amount = null, string selectedValue = null, string selectedText = null)
        {
            var parameter = db.ProductParameters.Include(entry=>entry.ProductParameterValues).FirstOrDefault(entry=>entry.Id == parameterId);
            var values =
                parameter.ProductParameterValues.OrderBy(entry=>entry.Order).Select(
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
                //StartText = selectedText ?? values.First().Text
            };
            var partial = ControllerContext.RenderPartialToString("_ProductOption", model);
            return Json(new { html = partial, values }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FeatureValues(string featureId)
        {
            var values = db.ProductParameterValues
                .Where(entry => entry.ProductParameterId == featureId)
                .Select(entry => entry.ParameterValue)
                .Distinct()
                .Select(entry => new { value = entry, data = entry }).ToList();
            return Json(values,JsonRequestBehavior.AllowGet);
        }

        public ActionResult NewFeature()
        {
            return PartialView("_NewProductFeature");
        }

        public ActionResult ProductParameterCategory(string id = null, string categoryId = null)
        {
            var productParameter = db.ProductParameters.Find(id) ?? new ProductParameter() { CategoryId = categoryId };
            return PartialView("_ProductParameterGroup", productParameter);
        }
        public ActionResult ProductParameterValue(string valueId, string parameterId, string categoryId)
        {
            var productParameter = db.ProductParameterValues.Find(valueId) ?? new ProductParameterValue() { ProductParameterId = parameterId };
            ViewBag.CategoryId = categoryId;
            return PartialView("_ProductParameterValue", productParameter);
        }

        public ActionResult CreateOrUpdate(string categoryId, string id = null)
        {
            ViewBag.Type = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Text = "Текст (задане значення)",
                    Value = typeof (string).ToString(),
                    Selected = true
                },
                new SelectListItem() {Text = "Ціле число", Value = typeof (int).ToString()},
                new SelectListItem() {Text = "Десятичне число", Value = typeof (double).ToString()}
            };
            var productParameter = db.ProductParameters.FirstOrDefault(entry => entry.Id == id) ??
                                   new ProductParameter()
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
                new SelectListItem()
                {
                    Text = "Текст (задане значення)",
                    Value = typeof (string).ToString(),
                    Selected = true
                },
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
                if (productParameterValue.Id == null)
                {
                    productParameterValue.Id = Guid.NewGuid().ToString();
                    db.ProductParameterValues.Add(productParameterValue);
                }
                else
                {
                    db.Entry(productParameterValue).State = EntityState.Modified;
                }
                db.SaveChanges();
                TempData["SuccessMessage"] = "Зміни збережено";
                return RedirectToAction("Index", new { categoryId });
            }
            TempData["ErrorMessage"] = "Невірно задані дані";
            return RedirectToAction("Index", new { categoryId });
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
