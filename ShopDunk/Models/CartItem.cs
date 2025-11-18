using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopDunk.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public string Color { get; set; }
        public string Storage { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
    }
}