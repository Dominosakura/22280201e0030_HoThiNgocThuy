using Candy.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Candy.Controllers
{
    public class SellerController : Controller
    {
        DECOMMERCEEntities3 db = new DECOMMERCEEntities3();

        private ActionResult IsSeller()
        {
           
            var user = Session["User"] as User;

          
            if (user == null || user.IdRole != 2 || user.Status == false)
            {
                return RedirectToAction("DangNhap", "User");
            }

            return null;
        }

     
        public ActionResult Index()
        {
           
            var result = IsSeller();
            if (result != null) return result;

            return View();
        }

        public ActionResult SellerProducts()
        {
        
            var result = IsSeller();
            if (result != null) return result;

            var seller = Session["User"] as User;

            var productsForSeller = db.ProductImages
                .Include("Product")
                .Where(img => img.IsMainImage == true && img.Product.SellerID == seller.UserID)
                .ToList();

            return View(productsForSeller);
        }

        // GET: Seller/AddProduct
        public ActionResult AddProduct()
        {
            var result = IsSeller();
            if (result != null) return result;

            ViewBag.Categories = db.ProductCategories.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(Product product, string CategoryName, HttpPostedFileBase mainImage, HttpPostedFileBase[] additionalImages)
        {
            var result = IsSeller();
            if (result != null) return result;

            var seller = Session["User"] as User;
            var category = db.ProductCategories.SingleOrDefault(c => c.CategoryName == CategoryName);
            if (category == null)
            {
                ModelState.AddModelError("", "Invalid category");
                ViewBag.Categories = db.ProductCategories.ToList();
                return View(product);
            }

            product.SellerID = seller.UserID;
            product.CategoryID = category.CategoryID;
            product.CreatedDate = DateTime.Today;
            product.Status = "Active";

            db.Products.Add(product);
            db.SaveChanges(); 

       
            if (mainImage != null && mainImage.ContentLength > 0)
            {
                string mainImagePath = Path.Combine(Server.MapPath("~/Images/"), Path.GetFileName(mainImage.FileName));
                mainImage.SaveAs(mainImagePath);

                var mainProductImage = new ProductImage
                {
                    ProductID = product.ProductID,
                    ImageURL = Path.GetFileName(mainImage.FileName),
                    IsMainImage = true
                };

                db.ProductImages.Add(mainProductImage);
            }

         
            if (additionalImages != null && additionalImages.Length > 0)
            {
                foreach (var image in additionalImages)
                {
                    if (image != null && image.ContentLength > 0)
                    {
                        string additionalImagePath = Path.Combine(Server.MapPath("~/Images/"), Path.GetFileName(image.FileName));
                        image.SaveAs(additionalImagePath);

                        var additionalProductImage = new ProductImage
                        {
                            ProductID = product.ProductID,
                            ImageURL = Path.GetFileName(image.FileName),
                            IsMainImage = false
                        };

                        db.ProductImages.Add(additionalProductImage);
                    }
                }
            }

            db.SaveChanges();

            return RedirectToAction("SellerProducts", "Seller");
        }

        public ActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            var result = IsSeller();
            if (result != null) return result;

          
            if (newStatus == "Pending Confirmation" || newStatus == "Awaiting Pickup" ||
                newStatus == "Awaiting Delivery" || newStatus == "Delivered" ||
                newStatus == "Reviewed" || newStatus == "Cancelled")
            {
                var order = db.Orders.SingleOrDefault(o => o.OrderID == orderId);

                if (order != null)
                {
                    order.Status = newStatus; 
                    db.SaveChanges(); 
                    return RedirectToAction("OrderList"); 
                }
            }

            ViewBag.ErrorMessage = "Trạng thái đơn hàng không hợp lệ hoặc đơn hàng không tồn tại!";
            return RedirectToAction("OrderList");
        }

    
        public ActionResult DeleteProduct(int id)
        {
        
            var result = IsSeller();
            if (result != null) return result;

            string username = Session["User"] as string;
            var seller = db.Users.FirstOrDefault(u => u.Username == username);
            var product = db.Products.SingleOrDefault(p => p.ProductID == id && p.SellerID == seller.UserID);

            if (product == null)
            {
                return HttpNotFound("Product not found or access denied");
            }

            return View(product); 
        }


        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProductConfirmed(int? id)
        {

            var result = IsSeller();
            if (result != null) return result;

            var seller = Session["User"] as User;
            var product = db.Products.SingleOrDefault(p => p.ProductID == id && p.SellerID == seller.UserID);

            if (product == null)
            {
                return HttpNotFound("Product not found or access denied");
            }

         
            var productImages = db.ProductImages.Where(pi => pi.ProductID == product.ProductID).ToList();
            foreach (var image in productImages)
            {
                string imagePath = Path.Combine(Server.MapPath("~/Images/"), image.ImageURL);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                db.ProductImages.Remove(image);
            }

           
            db.Products.Remove(product);
            db.SaveChanges();

            return RedirectToAction("SellerProducts", "Seller"); 
        }


        public ActionResult EditProduct(int id)
        {
            var result = IsSeller();
            if (result != null) return result;

            var seller = Session["User"] as User;
            var product = db.Products.SingleOrDefault(p => p.ProductID == id && p.SellerID == seller.UserID);
            if (product == null)
            {
                return HttpNotFound("Product not found or access denied");
            }

            ViewBag.Categories = db.ProductCategories.ToList();
            ViewBag.AdditionalImages = db.ProductImages.Where(pi => pi.ProductID == product.ProductID && !pi.IsMainImage == true).ToList();
            return View(product);
        }

      
        [HttpPost]
        public ActionResult EditProduct(Product product, string CategoryName, HttpPostedFileBase mainImage, IEnumerable<HttpPostedFileBase> additionalImages)
        {
            var result = IsSeller();
            if (result != null) return result;

            var seller = Session["User"] as User;
            var existingProduct = db.Products.SingleOrDefault(p => p.ProductID == product.ProductID && p.SellerID == seller.UserID);
            if (existingProduct == null)
            {
                return HttpNotFound("Product not found or access denied");
            }


            existingProduct.ProductName = product.ProductName;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;


            var category = db.ProductCategories.SingleOrDefault(c => c.CategoryName == CategoryName);
            if (category != null)
            {
                existingProduct.CategoryID = category.CategoryID;
            }


            if (mainImage != null && mainImage.ContentLength > 0)
            {
                var oldMainImage = db.ProductImages.FirstOrDefault(pi => pi.ProductID == existingProduct.ProductID && pi.IsMainImage == true);
                if (oldMainImage != null)
                {
                    string oldMainImagePath = Path.Combine(Server.MapPath("~/Images/"), oldMainImage.ImageURL);
                    if (System.IO.File.Exists(oldMainImagePath))
                    {
                        System.IO.File.Delete(oldMainImagePath);
                    }
                    db.ProductImages.Remove(oldMainImage);
                }

                string mainImagePath = Path.Combine(Server.MapPath("~/Images/"), Path.GetFileName(mainImage.FileName));
                mainImage.SaveAs(mainImagePath);

                var newMainImage = new ProductImage
                {
                    ProductID = existingProduct.ProductID,
                    ImageURL = Path.GetFileName(mainImage.FileName),
                    IsMainImage = true
                };
                db.ProductImages.Add(newMainImage);
            }

            if (additionalImages != null)
            {
                foreach (var image in additionalImages)
                {
                    if (image != null && image.ContentLength > 0)
                    {
                        string additionalImagePath = Path.Combine(Server.MapPath("~/Images/"), Path.GetFileName(image.FileName));
                        image.SaveAs(additionalImagePath);

                        var additionalProductImage = new ProductImage
                        {
                            ProductID = existingProduct.ProductID,
                            ImageURL = Path.GetFileName(image.FileName),
                            IsMainImage = false
                        };
                        db.ProductImages.Add(additionalProductImage);
                    }
                }
            }

            db.SaveChanges();

            return RedirectToAction("SellerProducts", "Seller");
        }

  
        public ActionResult AddQuantity(int productId)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction("Index");
            }

         
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddQuantity(int productId, int quantityToAdd)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction("Index");
            }

         
            product.StockQuantity += quantityToAdd;

            db.SaveChanges();

            TempData["SuccessMessage"] = "Quantity updated successfully!";
            return RedirectToAction("SellerProducts", "Seller");
        }

        public ActionResult OrderList(int? page)
        {
            var result = IsSeller();
            if (result != null) return result;

            var seller = Session["User"] as User;
            int currentSellerId = seller.UserID;
            var sellerProductIds = db.Products
                                     .Where(p => p.SellerID == currentSellerId)
                                     .Select(p => p.ProductID)
                                     .ToList();

            var sellerOrderIds = db.OrderDetails
                           .Where(od => od.ProductID.HasValue && sellerProductIds.Contains(od.ProductID.Value))
                           .Select(od => od.OrderID)
                           .Distinct()
                           .ToList();


         
            var sellerOrders = db.Orders
                                 .Where(o => sellerOrderIds.Contains(o.OrderID))
                                 .ToList();

            var sellerProduct = db.Products
                         .Where(p => p.SellerID == currentSellerId)
                         .Select(p => p.ProductID)
                         .ToList();

          
            int totalFavoritesBySeller = db.Favorites
                                           .Count(w => sellerProduct.Contains(w.ProductID.Value));

            int totalOrders = sellerOrders.Count; 
            int totalProduct = sellerProductIds.Count; 
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalProduct = totalProduct;
            ViewBag.TotalFavoritesBySeller = totalFavoritesBySeller;


            // Phân trang
            int iPageNum = (page ?? 1);
            int iPageSize = 12;

            return View(sellerOrders.OrderBy(o => o.OrderID).ToPagedList(iPageNum, iPageSize));
        }


    }
}


   
