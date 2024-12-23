using Candy.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;


namespace Candy.Areas.Admin.Controllers
{
   
    public class HomeController : Controller
    {
        DECOMMERCEEntities3 db = new DECOMMERCEEntities3();

        // GET: Admin/Home

        public ActionResult Guide(int page = 1)
        {
            ViewBag.Page = page;
            return View();
        }

        public ActionResult Index()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

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
                                 .Where(img => img.ProductID == p.ProductID)
                                 .Select(img => img.ImageURL)
                                 .FirstOrDefault()
                })
                .ToList();

            return View(bestSellers);
        }


      
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(string Username, string Password)
        {
            var user = db.UserAdmins.SingleOrDefault(u => u.UserName == Username); 

            if (user != null)
            {

                var passwordHasher = new PasswordHasher();
                var result = passwordHasher.VerifyHashedPassword(user.Password, Password);

                if (result == PasswordVerificationResult.Success)
                {
                  
                  
                    db.SaveChanges();

                    Session["Admin"] = user;
                 

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                  
                    ViewBag.ThongBao = "The username or password is incorrect!";
                    return View();
                }
            }
            else
            {
                ViewBag.ThongBao = "The username or password is incorrect!";
                return View();
            }
        }



      
        public ActionResult DangKy()
        {
            return View();
        }




        [HttpPost]
        public ActionResult DangKy(FormCollection collection, UserAdmin user)
        {
            // Lấy dữ liệu từ FormCollection
            var sFullName = collection["FullName"];
            var sUsername = collection["Username"];
            var sPassword = collection["Password"];
            var sPasswordConfirm = collection["PasswordConfirm"];
            var sEmail = collection["Email"];
            var sPhoneNumber = collection["PhoneNumber"];

            // Kiểm tra dữ liệu nhập vào
            if (string.IsNullOrEmpty(sFullName))
            {
                ViewData["errFullName"] = "Full name cannot be empty.";
            }
            else if (string.IsNullOrEmpty(sUsername))
            {
                ViewData["errUsername"] = "Username cannot be empty.";
            }
            else if (string.IsNullOrEmpty(sPassword))
            {
                ViewData["errPassword"] = "Password cannot be empty.";
            }
            else if (string.IsNullOrEmpty(sPasswordConfirm))
            {
                ViewData["errPasswordConfirm"] = "Password confirmation cannot be empty.";
            }
            else if (sPassword != sPasswordConfirm)
            {
                ViewData["errPasswordConfirm"] = "Passwords do not match.";
            }
            else if (string.IsNullOrEmpty(sEmail))
            {
                ViewData["errEmail"] = "Email cannot be empty.";
            }
            else if (!IsValidEmail(sEmail))
            {
                ViewData["errEmail"] = "Invalid email format.";
            }
            else if (string.IsNullOrEmpty(sPhoneNumber))
            {
                ViewData["errPhoneNumber"] = "Phone number cannot be empty.";
            }
            else if (!int.TryParse(sPhoneNumber, out int phoneNumber))
            {
                ViewData["errPhoneNumber"] = "Phone number must be a valid number.";
            }
            else
            {
                try
                {
                    // Hash mật khẩu
                    var passwordHasher = new PasswordHasher();
                    user.Password = passwordHasher.HashPassword(sPassword);

                    // Gán dữ liệu vào model UserAdmin
                    user.FullName = sFullName;
                    user.UserName = sUsername;
                    user.Email = sEmail;
                    user.Phone = phoneNumber; // Phone đã kiểm tra là số hợp lệ

                    // Lưu vào cơ sở dữ liệu
                    db.UserAdmins.Add(user);
                    db.SaveChanges();

                    // Chuyển hướng đến trang đăng nhập
                    return RedirectToAction("Login");
                }
                catch (DbEntityValidationException ex)
                {
                    // Hiển thị chi tiết lỗi validation từ Entity Framework
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ViewBag.Message += $"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}<br />";
                        }
                    }
                    return View();
                }
                catch (Exception ex)
                {
                    // Xử lý các lỗi khác
                    ViewBag.Message = "An error occurred: " + ex.Message;
                    return View();
                }
            }

            // Trả lại View với lỗi nếu có
            return View();
        }

        // Hàm kiểm tra định dạng email
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }




        public ActionResult OrderList(int? page)
        {
            int totalBuyers = db.Users.Count(kh => kh.IdRole == 1);
            int totalSellers = db.Users.Count(kh => kh.IdRole == 2);
            int totalUsers = totalBuyers + totalSellers;
            int totalOrders = db.Orders.Count();
            int totalProduct = db.Products.Count();

            ViewBag.TotalUser = totalUsers;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalProduct = totalProduct;
            int iPageNum = (page ?? 1);
            int iPgaeSize = 12;
            return View(db.Orders.ToList().OrderBy(dh => dh.OrderID).ToPagedList(iPageNum, iPgaeSize)); 
          
        }

        public ActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            // Kiểm tra nếu trạng thái mới hợp lệ
            if (newStatus == "Pending Confirmation" || newStatus == "Awaiting Pickup" ||
                newStatus == "Awaiting Delivery" || newStatus == "Delivered" ||
                newStatus == "Reviewed" || newStatus == "Cancelled")
            {
                var order = db.Orders.SingleOrDefault(o => o.OrderID == orderId);

                if (order != null)
                {
                    order.Status = newStatus; // Cập nhật trạng thái đơn hàng
                    db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                    return RedirectToAction("OrderList"); // Quay lại danh sách đơn hàng
                }
            }

            // Nếu trạng thái không hợp lệ hoặc đơn hàng không tồn tại
            ViewBag.ErrorMessage = "Trạng thái đơn hàng không hợp lệ hoặc đơn hàng không tồn tại!";
            return RedirectToAction("OrderList");
        }


        public ActionResult Profile()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("DangNhap");
            }
            var user = (UserAdmin)Session["Admin"];

            if (user != null)
            {
                return View(user);
            }
            else
            {
                ViewBag.ErrorMessage = "User not found!";
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpPost]
        public ActionResult UploadAvatar(HttpPostedFileBase avatar)
        {

            var user = (UserAdmin)Session["Admin"];
            // Check if an avatar file is selected
            if (avatar != null && avatar.ContentLength > 0)
            {
                // Check the file size (limit is 2MB)
                if (avatar.ContentLength > (2 * 1024 * 1024)) // 2MB
                {
                    ViewBag.Message = "The image size should not exceed 2MB.";
                    return RedirectToAction("Profile");
                }

                // Check if the file extension is valid
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(avatar.FileName).ToLower();

                if (!validExtensions.Contains(fileExtension))
                {
                    ViewBag.Message = "Only image formats .jpg, .jpeg, .png, .gif are accepted.";
                    return RedirectToAction("Profile");
                }

                // Path to save the avatar in the ~/Images/Avatar directory
                string fileName = Path.GetFileName(avatar.FileName);
                string path = Path.Combine(Server.MapPath("~/Images/Avatar"), fileName);

                // Save the image file
                avatar.SaveAs(path);

                // Update the avatar path in the database
                user.Avartar = fileName;
                db.SaveChanges(); // Save changes to the database

                ViewBag.Message = "Avatar uploaded successfully!";
            }
            else
            {
                ViewBag.Message = "Please select an image to upload.";
            }

            return RedirectToAction("Profile"); // Redirect back to the profile page
        }

        // Logout Method
        public ActionResult Logout()
        {

            Session.Clear();
            return RedirectToAction("Login");
        }


        public ActionResult EditProfile()
        {
            var user = (UserAdmin)Session["Admin"];

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user); // Truyền thông tin người dùng sang View
        }

        // POST: EditProfile
        [HttpPost]
        public ActionResult EditProfile( string FullName, string Email, int? PhoneNumber)
        {


            var user = (UserAdmin)Session["Admin"];

           
            user.FullName = FullName;
            user.Email = Email;
            user.Phone = PhoneNumber;


            try
            {
                db.SaveChanges(); // Save to the database
                ViewBag.Message = "Information updated successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "An error occurred: " + ex.Message;
            }

            return View(user);

        }
    }



}