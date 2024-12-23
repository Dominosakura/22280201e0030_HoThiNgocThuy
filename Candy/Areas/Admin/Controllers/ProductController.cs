using Candy.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Candy.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        DECOMMERCEEntities3 db = new DECOMMERCEEntities3();
        // GET: Admin/Product
        public ActionResult Index( int? page)
        {
            int iPageNum = (page ?? 1);
            int iPageSize = 10;

            return View(db.Products.ToList().OrderBy(p => p.ProductID).ToPagedList(iPageNum,iPageSize));
        }


        public ActionResult Details(int id)
        {
            var product = db.Products.SingleOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var seller = db.Users.SingleOrDefault(u => u.UserID == product.SellerID);
            if (seller == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            // Lấy tên người bán
            ViewBag.SellerFullName = seller.FullName;

            // Lấy tên danh mục sản phẩm
            var category = db.ProductCategories.SingleOrDefault(c => c.CategoryID == product.CategoryID);
            ViewBag.CategoryName = category.CategoryName ; // Tránh null nếu không tìm thấy

            return View(product); // Truyền đối tượng product vào View
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {

            var product = db.Products.SingleOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var seller = db.Users.SingleOrDefault(u => u.UserID == product.SellerID);
            if (seller == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            // Lấy tên người bán
            ViewBag.SellerFullName = seller.FullName;

            // Lấy tên danh mục sản phẩm
            var category = db.ProductCategories.SingleOrDefault(c => c.CategoryID == product.CategoryID);
            ViewBag.CategoryName = category.CategoryName; // Tránh null nếu không tìm thấy

            return View(product); // Truyền đối tượng product vào View
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirm(int id)
        {
            var product = db.Products.SingleOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                // Nếu không tìm thấy sản phẩm, trả về mã lỗi 404
                Response.StatusCode = 404;
                return Content("Sản phẩm không tồn tại.");
            }

            // Kiểm tra xem sản phẩm có trong chi tiết đơn hàng không
            var orderDetails = db.OrderDetails.Where(od => od.ProductID == id).ToList();
            if (orderDetails.Any())
            {
                // Nếu có chi tiết đơn hàng liên quan, hiển thị thông báo
                ViewBag.ErrorMessage = "Sản phẩm này đang có trong các đơn hàng. Vui lòng xóa các chi tiết đơn hàng trước khi xóa sản phẩm này.";
                return View("DeleteProduct", product);
            }

            try
            {
                // Bắt đầu xóa dữ liệu liên quan đến sản phẩm nếu không có chi tiết đơn hàng

                // Xóa hình ảnh của sản phẩm trong ProductImages
                var productImages = db.ProductImages.Where(pi => pi.ProductID == id).ToList();
                db.ProductImages.RemoveRange(productImages);

                // Xóa sản phẩm khỏi các bảng liên quan khác
                var favorites = db.Favorites.Where(f => f.ProductID == id).ToList();
                db.Favorites.RemoveRange(favorites);

                var cartItems = db.ShoppingCartItems.Where(sci => sci.ProductID == id).ToList();
                db.ShoppingCartItems.RemoveRange(cartItems);

                var reviews = db.Reviews.Where(r => r.ProductID == id).ToList();
                db.Reviews.RemoveRange(reviews);

                var userCoupons = db.Promotions.Where(uc => uc.ProductID == id).ToList();
                db.Promotions.RemoveRange(userCoupons);

                // Xóa sản phẩm trong bảng Products
                db.Products.Remove(product);

                // Lưu thay đổi
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có vấn đề khi xóa
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi xóa sản phẩm. Vui lòng thử lại sau.";
                return View("DeleteProduct", product);
            }

            return RedirectToAction("Index");
        }



        [HttpGet]
        public ActionResult Edit(int id)
        {
            var product = db.Products.SingleOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                Response.StatusCode = 404;
                return HttpNotFound("Không tìm thấy sản phẩm.");
            }

            ViewBag.Categories = db.ProductCategories.ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product, HttpPostedFileBase fFileUpload)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = db.Products.SingleOrDefault(p => p.ProductID == product.ProductID);
                if (existingProduct == null)
                {
                    return HttpNotFound("Không tìm thấy sản phẩm để cập nhật.");
                }

                // Cập nhật thông tin
                existingProduct.ProductName = product.ProductName;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.CategoryID = product.CategoryID;

                // Xử lý file ảnh
                if (fFileUpload != null)
                {
                    var fileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images"), fileName);

                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }
                    existingProduct.ProductImages.First().ImageURL = fileName;
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Categories = db.ProductCategories.ToList();
            return View(product);
        }



    }
}