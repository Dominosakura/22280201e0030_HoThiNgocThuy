using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Candy.Models
{
    public class HitCounterAttribute : ActionFilterAttribute
    {
        private static Dictionary<string, int> _userLoginCount = new Dictionary<string, int>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (session != null && session["Username"] != null)
            {
                string username = session["Username"].ToString();

             
                if (_userLoginCount.ContainsKey(username))
                {
                    _userLoginCount[username]++;
                }
                else
                {
                    _userLoginCount[username] = 1;
                }

                // Lưu thông tin vào ViewBag để hiển thị
                filterContext.Controller.ViewBag.UserLoginCount = _userLoginCount[username];

                // Lưu lượt đăng nhập vào file
                SaveLoginCountToFile();
            }

            base.OnActionExecuting(filterContext);
        }

        private void SaveLoginCountToFile()
        {
            string filePath = HttpContext.Current.Server.MapPath("~/App_Data/userLoginCount.txt");
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var entry in _userLoginCount)
                {
                    writer.WriteLine($"{entry.Key}: {entry.Value}");
                }
            }
        }
    }
}