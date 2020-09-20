using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Web.Areas.Admin.Controllers.Base;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class RegionsController : AdminController
    {
        public ActionResult Index(string name = null)
        {
            using (var db = new ApplicationDbContext())
            {
                var regions = db.Regions.Include(r => r.Parent).Where(entry => entry.Name_ua.Contains(name) || entry.Name_ru.Contains(name));
                return View(regions.ToList());
            }
        }

        // GET: /Admin/Regions/Details/5
        public ActionResult Details(int? id)
        {
            using (var db = new ApplicationDbContext())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Region region = db.Regions.Find(id);
                if (region == null)
                {
                    return HttpNotFound();
                }
                return View(region);
            }
        }

        // GET: /Admin/Regions/Create
        public ActionResult Create()
        {
            using (var db = new ApplicationDbContext())
            {
                var region = new Region()
                {
                    Id = db.Regions.Max(entry => entry.Id) + 1
                };
                return View(region);
            }
        }

        // POST: /Admin/Regions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id,ParentId,PostalCode,Name_ru,RegionType,RegionLevel,Name_ua,Url")] Region region)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    db.Regions.Add(region);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(region);
            }
        }

        // GET: /Admin/Regions/Edit/5
        public ActionResult Edit(int? id)
        {
            using (var db = new ApplicationDbContext())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Region region = db.Regions.Find(id);
                if (region == null)
                {
                    return HttpNotFound();
                }
                ViewBag.ParentId = new SelectList(db.Regions, "Id", "PostalCode", region.ParentId);
                return View(region);
            }
        }

        // POST: /Admin/Regions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ParentId,PostalCode,Name_ru,RegionType,RegionLevel,Name_ua,Url")] Region region)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    db.Entry(region).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.ParentId = new SelectList(db.Regions, "Id", "PostalCode", region.ParentId);
                return View(region);
            }
        }
    }
}
