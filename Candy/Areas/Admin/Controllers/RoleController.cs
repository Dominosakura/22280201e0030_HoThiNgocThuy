using Candy.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Candy.Areas.Admin.Controllers
{
    public class RoleController : Controller
    {
        DECOMMERCEEntities3 db = new DECOMMERCEEntities3();
        // GET: Admin/Role
        public ActionResult Index(int? page)
        {
            int iPageNum = (page ?? 1);
            int iPageSize = 12;

            return View(db.Users.ToList().OrderBy(u => u.UserID).ToPagedList(iPageNum, iPageSize));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var user = db.Users.SingleOrDefault(u => u.UserID == id);
            if (user == null)
            {
                return HttpNotFound("User not found for status toggle.");
            }

          
            user.Status = user.Status== false ? true : false;

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var user = db.Users.SingleOrDefault(u => u.UserID == id);
            if (user == null)
            {
                Response.StatusCode = 404;
                return Content("Tài khoản không tồn tại.");
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = db.Users.SingleOrDefault(u => u.UserID == id);
            if (user == null)
            {
                return HttpNotFound("User not found for deletion.");
            }

            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
            var user = db.Users.SingleOrDefault(u => u.UserID == id);
            if (user == null)
            {
                return HttpNotFound("User not found for editing.");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user, HttpPostedFileBase fFileUpload)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Users.SingleOrDefault(u => u.UserID == user.UserID);
                if (existingUser == null)
                {
                    return HttpNotFound("User not found for update.");
                }

                // Cập nhật thông tin người dùng
                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.FullName = user.FullName;
                existingUser.PhoneNumber = user.PhoneNumber;

                // Xử lý file ảnh đại diện
                if (fFileUpload != null)
                {
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/Avatar"), sFileName);

                    // Kiểm tra file trùng và lưu file
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }
                    existingUser.AvatarPath = sFileName;
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }



    }

}
