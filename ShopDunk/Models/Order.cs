using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopDunk.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}