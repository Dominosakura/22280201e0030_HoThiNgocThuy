using Candy.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace Candy.Controllers
{
    public class PageHomeController : Controller
    {
        DECOMMERCEEntities3 db = new DECOMMERCEEntities3();

        [HitCounter]
        public ActionResult Index(string returnUrl = null)
        {
         

            int totalBuyers = db.Users.Count(kh => kh.IdRole==1);
            int totalSellers = db.Users.Count(kh => kh.IdRole==2);
            int totalUsers = totalBuyers + totalSellers;

            int totalProduct = db.Products.Count();
            ViewBag.TotalProduct = totalProduct;
            ViewBag.TotalUsers = totalUsers;

          
            var bestSellerProductIds = db.OrderDetails
               .GroupBy(od => od.ProductID)
               .Select(g => new
               {
                   ProductID = g.Key,
                   TotalQuantity = g.Sum(od => od.Quantity)
               })
               .OrderByDescending(x => x.TotalQuantity)
               .Take(5)
               .Select(x => x.ProductID)
               .ToList();

          
            var bestSellers = db.Products
             .Where(p => bestSellerProductIds.Contains(p.ProductID))
             .Select(p => new BestSellerProductViewModel
             {
                 ProductID = p.ProductID,
                 ProductName = p.ProductName,
                 CategoryName = p.ProductCategory.CategoryName,
                 StockQuantity = p.StockQuantity,
                 Price = p.Price,
                 ImageURL = db.ProductImages
                      .Where(img => img.ProductID == p.ProductID && img.IsMainImage == true)
                      .Select(img => img.ImageURL)
                      .FirstOrDefault()
             })
             .ToList();

          
            ViewBag.BestSellers = bestSellers;

            
            int numberOfProductsToShow = 12;
            DateTime today = DateTime.Today;
            var productsWithMainImages = db.ProductImages
                .Include("Product")
                .Where(img => img.IsMainImage == true && DbFunctions.TruncateTime(img.Product.CreatedDate) == today)
                .Take(numberOfProductsToShow)
                .ToList();

            return View(productsWithMainImages);
        }

        public ActionResult NavPartial()
        {
            return View();
        }

        public ActionResult CategoryPartical()
        {
            var categories = db.ProductCategories
                                  .Include(c => c.Products)
                                  .ToList();

            return View(categories);
        }
    }
}
