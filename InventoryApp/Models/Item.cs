using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InventoryApp.Models
{
    public class Item
    {
        public Item()
        {

        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public int Threshold { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string Description { get; set; }

        public byte[] Picture { get; set; }
        public string MimeType { get; set; }
        
    }
}