using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopDunk.Models
{
    public class CartItem
    {
        public int CartItemID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }

        public virtual Product Product { get; set; }
    }
}