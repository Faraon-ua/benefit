﻿using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Web.Filters;

namespace Benefit.Web.Controllers
{
    public class ErrorController : Controller
    {
        [FetchCategories]
        [FetchSeller]
        public ActionResult NotFound()
        {
            var domainSeller = ViewBag.Seller as Seller;
            if (domainSeller != null)
            {
                if (domainSeller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default) ==
                    SellerEcommerceTemplate.MegaShop)
                {
                    return View("~/views/sellerarea/megashop/notfound.cshtml");
                }
            }
            return View();
        }
	}
}