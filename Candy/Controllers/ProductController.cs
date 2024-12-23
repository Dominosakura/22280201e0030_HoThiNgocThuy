using Candy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Candy.Controllers
{
    public class ProductController : Controller
    {
          DECOMMERCEEntities3 db = new DECOMMERCEEntities3();

        // GET: Product
        public ActionResult Index(string keyword, int? page)
        {
            
            int pageSize = 60;
            int pageNumber = page ?? 1;

            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                products = products.Where(p => p.ProductName.ToLower().Contains(keyword.ToLower()) ||
                                                p.Description.ToLower().Contains(keyword.ToLower()));
            }

            var totalProducts = products.Count();
            ViewBag.CurrentKeyword = keyword; 

            var paginatedProducts = products.OrderBy(p => p.ProductID)
                                            .Skip((pageNumber - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToList();

            ViewBag.TotalProducts = totalProducts;
            ViewBag.CurrentPage = pageNumber;

            var productImages = paginatedProducts
            .Select(p => p.ProductImages.FirstOrDefault(img => img.IsMainImage == true))
            .Where(img => img != null) 
            .ToList();

            return View(productImages);
        }

        public ActionResult Details(int id)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return HttpNotFound();
            }

          
            var images = db.ProductImages.Where(i => i.ProductID == id).ToList();

            ViewBag.Product = product;
            ViewBag.Images = images;

            return View();
        }


        public ActionResult CategoryByProduct(int categoryId)
        {
            var products = db.Products.Where(p => p.CategoryID == categoryId).ToList();
            var category = db.ProductCategories.FirstOrDefault(c => c.CategoryID == categoryId);

            ViewBag.CategoryName = category?.CategoryName;
            return View(products);
        }


        public ActionResult CategoryNav()
        {
            return View();
        }

        public string CheckProductStatus(int productId)
        {
            var product = db.Products.SingleOrDefault(p => p.ProductID == productId);

            if (product == null)
            {
                return "Unknown";
            }

            return product.StockQuantity == 0 ? "Inactive" : "Active";
        }



    }
}

