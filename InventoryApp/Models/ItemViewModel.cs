using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InventoryApp.Models
{
    public class ItemViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0, Int32.MaxValue, ErrorMessage = "Value should be greater than or equal to 0")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, Int32.MaxValue, ErrorMessage = "Value should be greater than or equal to 0")]
        public int Threshold { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }
}