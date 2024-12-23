using Candy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Candy.Controllers
{
    public class UserController : Controller
    {
       
        DECOMMERCEEntities3 db = new DECOMMERCEEntities3();

        // GET: User Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Registration
        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }



        [HttpPost]
        public ActionResult DangKy(FormCollection collection, User user)
        {
            var sFullName = collection["FullName"];
            var sUsername = collection["Username"];
            var sPassword = collection["Password"];
            var sPasswordConfirm = collection["PasswordConfirm"];
            var sEmail = collection["Email"];
            var sIdRole = collection["IdRole"]; // Để là kiểu string
            var sPhoneNumber = collection["PhoneNumber"];
            var sAddress = collection["Address"];

            // Kiểm tra hợp lệ dữ liệu (giống như mã cũ)

            if (sPassword == sPasswordConfirm)
            {
                try
                {
                    // Mã hóa mật khẩu trước khi lưu
                    var passwordHasher = new PasswordHasher();
                    user.Password = passwordHasher.HashPassword(sPassword); // Mã hóa mật khẩu

                    user.FullName = sFullName;
                    user.Username = sUsername;
                    user.Email = sEmail;

                    // Không ép kiểu, trực tiếp so sánh với chuỗi
                    if (sIdRole == "2")
                    {
                        user.IdRole = 2; // Seller
                    }
                    else if (sIdRole == "1")
                    {
                        user.IdRole = 1; // Buyer
                    }
                    else
                    {
                        ViewBag.Message = "Invalid role selection.";
                        return View();
                    }

                    user.PhoneNumber = sPhoneNumber;
                    user.Address = sAddress;

                    // Thiết lập Status mặc định là 0
                    user.Status = false;

                    db.Users.Add(user);
                    db.SaveChanges();

                    return RedirectToAction("DangNhap");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error: " + ex.Message;
                    return View();
                }
            }
            else
            {
                ViewBag.Message = "Password and confirm password do not match!";
            }
            return View();
        }





        // GET: Login
        public ActionResult DangNhap()
            {
                return View();
            }

        [HttpPost]
        public ActionResult DangNhap(string Username, string Password)
        {
            var user = db.Users.SingleOrDefault(u => u.Username == Username);


            if (user != null)
            {
                // Kiểm tra mật khẩu
                var passwordHasher = new PasswordHasher();
                var result = passwordHasher.VerifyHashedPassword(user.Password, Password);

                if (result == PasswordVerificationResult.Success)
                {
                    //Session["Username"] = user.Username;
                    //Session["Password"] = user.Password;
                    Session["User"] = user;
                    return RedirectToAction("Index", "PageHome");
                }
                else
                {
                    ViewBag.ThongBao = "Incorrect username or password!";

                    return View();
                }
            }
            else
            {
                ViewBag.ThongBao = "Incorrect username or password!";
                return View();
            }
        }


   
        public ActionResult Profile()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("DangNhap");
            }
            var user = (User)Session["User"];

            if (user != null)
            {
                return View(user);
            }
            else
            {
                ViewBag.ErrorMessage = "User not found!";
                return RedirectToAction("Index", "PageHome");
            }
        }


        [HttpPost]
        public ActionResult UploadAvatar(HttpPostedFileBase avatar)
        {
           
            string username = Session["Username"] as string;

            
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("DangNhap");
            }

            var user = db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return HttpNotFound();
            }

           
            if (avatar != null && avatar.ContentLength > 0)
            {
               
                if (avatar.ContentLength > (2 * 1024 * 1024)) 
                {
                    ViewBag.Message = "The image size should not exceed 2MB.";
                    return RedirectToAction("Profile");
                }

               
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(avatar.FileName).ToLower();

                if (!validExtensions.Contains(fileExtension))
                {
                    ViewBag.Message = "Only image formats .jpg, .jpeg, .png, .gif are accepted.";
                    return RedirectToAction("Profile");
                }

              
                string fileName = Path.GetFileName(avatar.FileName);
                string path = Path.Combine(Server.MapPath("~/Images/Avatar"), fileName);

               
                avatar.SaveAs(path);

               
                user.AvatarPath = fileName;
                db.SaveChanges(); 

                ViewBag.Message = "Avatar uploaded successfully!";
            }
            else
            {
                ViewBag.Message = "Please select an image to upload.";
            }

            return RedirectToAction("Profile"); 
        }

        // Logout Method
        public ActionResult Logout()
        {
          
            Session.Clear();
            return RedirectToAction("DangNhap");
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            var user = db.Users.SingleOrDefault(u => u.Email == email);

            if (user != null)
            {
                
                var token = Guid.NewGuid().ToString();

                // Lưu token vào cơ sở dữ liệu hoặc sử dụng nó trong một khoảng thời gian giới hạn (xem xét hết hạn)
                user.PasswordResetToken = token;
                db.SaveChanges();

                // Gửi liên kết reset mật khẩu qua email
                var resetLink = Url.Action("ResetPassword", "User", new { token = token }, protocol: Request.Url.Scheme);

                var smtpClient = new SmtpClient("smtp.yourmailserver.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("your-email@example.com", "your-email-password"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("your-email@example.com"),
                    Subject = "Đặt lại mật khẩu",
                    Body = $"Nhấn <a href='{resetLink}'>vào đây</a> để đặt lại mật khẩu của bạn.",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);
                smtpClient.Send(mailMessage);

                ViewBag.Message = "Hướng dẫn đặt lại mật khẩu đã được gửi đến email của bạn.";
                return View();
            }
            else
            {
                ViewBag.Message = "Email không tồn tại.";
                return View();
            }
        }

        public ActionResult ResetPassword(string token)
        {
            var user = db.Users.SingleOrDefault(u => u.PasswordResetToken == token);

            if (user != null)
            {
                return View();
            }
            else
            {
                ViewBag.Message = "Mã token không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("DangNhap");
            }
        }

        [HttpPost]
        public ActionResult ResetPassword(string token, string newPassword)
        {
            var user = db.Users.SingleOrDefault(u => u.PasswordResetToken == token);

            if (user != null)
            {
                var passwordHasher = new PasswordHasher();
                user.Password = passwordHasher.HashPassword(newPassword);
                user.PasswordResetToken = null; // Xóa token sau khi đã đặt lại mật khẩu
                db.SaveChanges();

                ViewBag.Message = "Mật khẩu đã được đặt lại thành công!";
                return RedirectToAction("DangNhap");
            }
            else
            {
                ViewBag.Message = "Mã token không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("DangNhap");
            }
        }
        // GET: EditProfile
        public ActionResult EditProfile()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("DangNhap");
            }

            var username = Session["Username"].ToString();
            var user = db.Users.SingleOrDefault(u => u.Username == username);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: EditProfile
        [HttpPost]
        public ActionResult EditProfile(int UserID, string FullName, string Email, string PhoneNumber, string Address)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("DangNhap");
            }

            var user = db.Users.SingleOrDefault(u => u.UserID == UserID);

            if (user == null)
            {
                return HttpNotFound();
            }

            
            user.FullName = FullName;
            user.Email = Email;
            user.PhoneNumber = PhoneNumber;
            user.Address = Address;

            try
            {
                db.SaveChanges();
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
