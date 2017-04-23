using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using InventoryApp.Models;
using InventoryApp.Repositories;

namespace InventoryApp.Controllers
{
    public class ItemsController : Controller
    {
        //private DatabaseContext db = new DatabaseContext();
        private readonly InventoryRepository _repository = new InventoryRepository();

        // GET: Items
        public ActionResult Index()
        {
            return View(_repository.GetInventory());
        }

        // GET: Items/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = _repository.GetItem(id.Value);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        //[SUpervisor, admin]
        /*[HttpPost]
        public ActionResult Purchase(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(repo.MakePurchase(id.Value, 5));
            //return View(item);
        }*/

        // GET: Items/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Items/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Quantity,Description,Threshold,Price")] ItemViewModel item, HttpPostedFileBase imageFile)
        {
            //http://stackoverflow.com/questions/21682581/return-error-message-with-actionresult

            if (ModelState.IsValid)
            {
                _repository.CreateItem(item, imageFile);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        public JsonResult QueryItem(string q)
        {
            var query = _repository.FindItems(q);
            var results = query.Select(i => new { Name = i.Name, ItemId = i.Id });
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItem(int id)
        {
            Item item = _repository.GetItem(id);
            var results = new
            {
                name = item.Name,
                description = item.Description,
                available = item.Quantity,
                price = item.Price,
            };
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Image(int? id) {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = _repository.GetItem(id.Value);
            if (item == null)
            {
                return HttpNotFound();
            }
            if (item.Picture == null)
            {
                return File("~/Content/Images/noimage.png", "image/png");
            }
            return File(item.Picture, item.MimeType);
        }

        // GET: Items/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = _repository.GetItem(id.Value);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: Items/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Quantity,Description,Threshold,Price")] Item item, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                _repository.EditItem(item, imageFile);
                return RedirectToAction("Index");
            }
            return View(item);
        }

        // GET: Items/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = _repository.GetItem(id.Value);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _repository.RemoveItem(id);
            return RedirectToAction("Index");
        }

        /*protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }*/
    }
}
