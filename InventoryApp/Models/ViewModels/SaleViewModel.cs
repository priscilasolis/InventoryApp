using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace InventoryApp.Models
{
    public class SaleViewModel
    {
        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public virtual Item Item { get; set; }

        [Required]
        [Range(1, Int32.MaxValue, ErrorMessage = "Value should be greater than 0")]
        public int Quantity { get; set; }
    }
}