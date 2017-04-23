using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace InventoryApp.Models
{
    public class Order
    {
        public Order()
        {

        }

        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, Int32.MaxValue, ErrorMessage = "Value should be greater than 0")]
        public int Quantity { get; set; }
        public bool IsActive { get; set; }

        public OrderType Type { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public virtual Item Item { get; set; }
        
        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public enum OrderType
        {
            Purchase,
            Sale
        }

    }
}