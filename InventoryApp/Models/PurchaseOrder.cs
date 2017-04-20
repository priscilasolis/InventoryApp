using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace InventoryApp.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int Id { get; set; }

        public int Quantity { get; set; }
        public bool IsActive { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("ItemId")]
        public int ItemId { get; set; }

        public virtual Item Item { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}