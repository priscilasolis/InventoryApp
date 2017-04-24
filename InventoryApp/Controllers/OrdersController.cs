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
        private readonly InventoryRepository _repository = new InventoryRepository();

        // GET: Orders
        [Authorize]
        public ActionResult Index()
        {
            ApplicationUser currentUser = _repository.UserManager.FindById(User.Identity.GetUserId());
            List<string> roles = _repository.UserManager.GetRoles(User.Identity.GetUserId()).ToList();

            if (roles.Any(i => i == "Administrator" || i == "Supervisor"))
            {
                return View(_repository.GetOrders());
            }
            else if (roles.Contains("User"))
            {
                return RedirectToAction("UserIndex");
            }

            return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }


        [Authorize]
        public ActionResult UserIndex()
        {
            ApplicationUser currentUser = _repository.UserManager.FindById(User.Identity.GetUserId());
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
            Order order = _repository.GetOrder(id);
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
                ApplicationUser currentUser = _repository.UserManager.FindById(User.Identity.GetUserId());

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

        public ActionResult CreatePurchase()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public JsonResult CreatePurchases(IEnumerable<PurchaseViewModel> orders)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser currentUser = _repository.UserManager.FindById(User.Identity.GetUserId());

                List<int> failedOrders = new List<int>();
                foreach (PurchaseViewModel order in orders)
                {
                    bool orderSucess = _repository.CreatePurchase(order.ItemId, order.Quantity, currentUser);
                    if (!orderSucess) failedOrders.Add(order.ItemId);
                }

                return Json(new { error = false, failedOrders = failedOrders });
            }

            return Json(new { error = true });
        }

        // GET: Orders/Delete/5
        [Authorize]
        public ActionResult CancelOrder(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = _repository.GetOrder(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (!order.IsActive) return HttpNotFound("The order you're looking for has been already cancelled");

            bool isSuper = _repository.UserManager.IsInRole(User.Identity.GetUserId(), "Administrator");
            isSuper = isSuper || _repository.UserManager.IsInRole(User.Identity.GetUserId(), "Supervisor");
            bool isUser = _repository.UserManager.IsInRole(User.Identity.GetUserId(), "User");

            ViewBag.Hidden = "hidden";
            if (order.Type == Order.OrderType.Purchase && isSuper)
            {
                return View(order);
            }
            else if (order.Type == Order.OrderType.Sale && isSuper)
            {
                return View(order);
            }

            return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpPost, ActionName("CancelOrder")]
        [Authorize]
        public ActionResult CancelOrderConfirmed(int id)
        {
            bool success = _repository.CancelOrder(id);
            if (!success)
            {
                return View(_repository.GetOrder(id));
            }
            return RedirectToAction("Index");
        }
    }
}
