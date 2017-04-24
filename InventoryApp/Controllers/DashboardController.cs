using InventoryApp.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InventoryApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly InventoryRepository _repository = new InventoryRepository();

        public ActionResult MostSold()
        {
            return View(_repository.GetMostSoldWeek());
        }

        public ActionResult LowThreshold()
        {

            return View(_repository.GetItemsByThreshold());
        }
        
    }
}

