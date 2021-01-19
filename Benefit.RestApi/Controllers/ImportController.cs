using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Import;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Benefit.RestApi.Controllers
{
    public class ImportController : Controller
    {
        public ActionResult ProcessImport(string sellerId, SyncType type)
        {
            using (var db = new ApplicationDbContext())
            {
                var importTask = db.ExportImports
                .AsNoTracking()
                .FirstOrDefault(entry => entry.SellerId == sellerId && entry.SyncType == type);
                if (importTask == null)
                {
                    return Json(new { error = "Дані іморту ще не задано" }, JsonRequestBehavior.AllowGet);
                }
                if (importTask.IsImport)
                {
                    return Json(new { error = "Імпорт знаходиться в процессі обробки" }, JsonRequestBehavior.AllowGet);
                }

                Task.Run(() => ImportServiceFactory.GetImportServiceInstance(importTask.SyncType).Import(importTask.Id));
                return Json(new { message = "Іморт запущено" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}