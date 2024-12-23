using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Candy.Models
{
    public class BestSellerProductViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int ?StockQuantity { get; set; }
        public decimal ?Price { get; set; }
        public string ImageURL { get; set; }
    }
}