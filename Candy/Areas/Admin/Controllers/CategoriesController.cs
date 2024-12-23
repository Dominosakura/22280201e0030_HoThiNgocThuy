using Candy.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Candy.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        DECOMMERCEEntities3 db = new DECOMMERCEEntities3();
        // GET: Admin/Categories
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult CategoriesList(int ? page)
        {
            int iPageNum = page ?? 1;
            int iPageSize = 10;
            return View(db.ProductCategories.ToList().OrderBy(c => c.CategoryID).ToPagedList(iPageNum, iPageSize));
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductCategory category, HttpPostedFileBase fFileUpload)
        {
            // Kiểm tra xem có chọn ảnh bìa không
            if (fFileUpload == null)
            {
                ViewBag.ThongBao = "Hãy chọn ảnh bìa";
                return View(category);
            }
            else
            {
                // Kiểm tra nếu dữ liệu hợp lệ
                if (ModelState.IsValid)
                {
                    try
                    {
                        // Lấy tên file ảnh
                        var fileName = Path.GetFileName(fFileUpload.FileName);
                        var path = Path.Combine(Server.MapPath("~/Images/"), fileName);

                        // Kiểm tra nếu file chưa tồn tại thì lưu
                        if (!System.IO.File.Exists(path))
                        {
                            fFileUpload.SaveAs(path);
                        }

                        category.ImageURL = fileName;
                        category.CreatedBy = "System";
                        category.UpdatedBy = Session["Username"].ToString();
                        category.CreatedDate = DateTime.Now;
                        category.UpdatedDate = null;

                        // Thêm đối tượng vào cơ sở dữ liệu và lưu thay đổi
                        db.ProductCategories.Add(category);
                        db.SaveChanges();

                        return RedirectToAction("CategoriesList");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        // Nếu có lỗi xác thực trong Entity Framework
                        foreach (var validationError in ex.EntityValidationErrors)
                        {
                            foreach (var error in validationError.ValidationErrors)
                            {
                                ModelState.AddModelError("", $"{error.PropertyName}: {error.ErrorMessage}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Bắt các lỗi khác, ví dụ lỗi file hoặc lưu dữ liệu
                        ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                    }
                }
                else
                {
                    // Nếu ModelState không hợp lệ
                    ModelState.AddModelError("", "Dữ liệu không hợp lệ, vui lòng kiểm tra lại.");
                }
            }

            // Trả lại View với thông báo lỗi nếu có
            return View(category);
        }


       
        public ActionResult Details(int id)
        {
            var category = db.ProductCategories.SingleOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(category);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            var category = db.ProductCategories.SingleOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirm(int id)
        {
            // Tìm danh mục dựa trên ID
            var category = db.ProductCategories.SingleOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                // Nếu không tìm thấy danh mục, trả về mã lỗi 404
                Response.StatusCode = 404;
                return Content("Danh mục không tồn tại.");
            }

            // Kiểm tra xem có sản phẩm nào trong danh mục không
            var productsInCategory = db.Products.Where(p => p.CategoryID == id).ToList();
            if (productsInCategory.Any())
            {
                // Nếu có sản phẩm, hiển thị thông báo yêu cầu xóa các sản phẩm trước
                ViewBag.ErrorMessage = "Danh mục này đang chứa sản phẩm. Vui lòng xóa tất cả sản phẩm trong danh mục trước khi xóa danh mục này.";
                return View("Delete", category);
            }

            try
            {
                // Nếu không có sản phẩm nào trong danh mục, tiến hành xóa danh mục
                db.ProductCategories.Remove(category);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có vấn đề khi xóa
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi xóa danh mục. Vui lòng thử lại sau.";
                return View("Delete", category);
            }

            return RedirectToAction("CategoriesList");
        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
            var category = db.ProductCategories.SingleOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                Response.StatusCode = 404;
                return Content("Danh mục không tồn tại.");
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductCategory category, HttpPostedFileBase fFileUpload)
        {
            var existingCategory = db.ProductCategories.SingleOrDefault(c => c.CategoryID == category.CategoryID);
            if (existingCategory == null)
            {
                Response.StatusCode = 404;
                return Content("Danh mục không tồn tại.");
            }

            if (fFileUpload != null)
            {
                var fileName = Path.GetFileName(fFileUpload.FileName);
                var path = Path.Combine(Server.MapPath("~/Images/"), fileName);

                if (!System.IO.File.Exists(path))
                {
                    fFileUpload.SaveAs(path);
                }

                existingCategory.ImageURL = fileName;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingCategory.CategoryName = category.CategoryName;
                    existingCategory.Description = category.Description;

                 
                    var username = Session["Username"]?.ToString();
                    if (string.IsNullOrEmpty(username))
                    {
                        ModelState.AddModelError("", "Không thể lấy thông tin người dùng từ session.");
                        return View(existingCategory);
                    }
                    existingCategory.UpdatedBy = username;
                    existingCategory.UpdatedDate = DateTime.Now;

                    db.Entry(existingCategory).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("CategoriesList");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationError in ex.EntityValidationErrors)
                    {
                        foreach (var error in validationError.ValidationErrors)
                        {
                            ModelState.AddModelError("", $"{error.PropertyName}: {error.ErrorMessage}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError("", "Dữ liệu không hợp lệ, vui lòng kiểm tra lại.");
            }

            return View(existingCategory);
        }






    }
}