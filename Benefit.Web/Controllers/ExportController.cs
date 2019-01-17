using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;

namespace Benefit.Web.Controllers
{
    public class ExportController : Controller
    {
        ImportExportService ImportExportService = new ImportExportService();


        public ActionResult Index(string id)
        {
            var yml = ImportExportService.ExportSeller(id);
            return Content(yml, "text/xml");
        }
    }
}