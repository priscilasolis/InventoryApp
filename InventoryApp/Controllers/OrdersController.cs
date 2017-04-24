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
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace InventoryApp.Controllers
{
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly InventoryRepository _repository = new InventoryRepository();

        // GET: Orders
        [Authorize(Roles = "Administrator,Supervisor")]
        public ActionResult Index()
        {
            var orders = db.Orders.Include(o => o.ApplicationUser).Include(o => o.Item);
            return View(orders.ToList());
        }


        [Authorize]
        public ActionResult UserIndex()
        {
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());

            var r = _repository.GetSalesUser(currentUser);
            
            return View(r);
        }



        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [Authorize]
        public ActionResult CreateSale()
        {
            //ViewBag.ItemId = new SelectList(db.Inventory, "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize]
        public JsonResult CreateSales(IEnumerable<SaleViewModel> orders)
        {
            if (ModelState.IsValid) {
                UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());

                List<int> failedOrders = new List<int>();
                foreach (SaleViewModel order in orders)
                {
                    bool orderSucess = _repository.CreateSale(order.ItemId, order.Quantity, currentUser);
                    if (!orderSucess) failedOrders.Add(order.ItemId);
                }

                return Json(new { error = false, failedOrders = failedOrders });
            }
            
            return Json(new { error = true });
        }

        public JsonResult QuerySale(int itemId, int quantity)
        {
            if (_repository.IsSalePossible(itemId, quantity)) {
                Item item = _repository.GetItem(itemId);
                var itemJson = new
                {
                    id = item.Id,
                    name = item.Name,
                    description = item.Description,
                    price = item.Price,
                    availableQuantity = item.Quantity,
                    quantity = quantity,
                    isValid = true
                };
                return Json(itemJson, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
        }

        // GET: Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.ApplicationUserId = new SelectList(db.Users, "Id", "Email", order.ApplicationUserId);
            ViewBag.ItemId = new SelectList(db.Inventory, "Id", "Name", order.ItemId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Quantity,IsActive,Type,Date,ItemId,ApplicationUserId")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ApplicationUserId = new SelectList(db.Users, "Id", "Email", order.ApplicationUserId);
            ViewBag.ItemId = new SelectList(db.Inventory, "Id", "Name", order.ItemId);
            return View(order);
        }

        // GET: Orders/Delete/5
        [Authorize]
        public ActionResult CancelSale(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [HttpPost, ActionName("CancelSale")]
        [Authorize]
        public ActionResult CancelSaleConfirmed(int id)
        {
            _repository.CancelSale(id);
            return RedirectToAction("Index");
        }

        // GET: Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
            db.SaveChanges();
            return RedirectToAction("Index");
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
