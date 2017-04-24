using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace InventoryApp.Models
{
    public class ItemSoldViewModel
    {
        public string Name { get; set; }
        [DisplayName("Quantity sold")]
        public int QuantitySold { get; set; }
    }
}