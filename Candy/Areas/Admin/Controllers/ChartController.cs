using Candy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Candy.Areas.Admin.Controllers
{

    

    public class ChartController : Controller
    {
        DECOMMERCEEntities3 db = new DECOMMERCEEntities3();
        // GET: Admin/Chart
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TotalRevenue()
        {
          
            var totalRevenue = db.Orders
                                 .Where(o => o.Status == "Delivered")  // Lọc các đơn hàng đã giao
                                 .Sum(o => o.TotalAmount);  // Tính tổng doanh thu

            // Gán giá trị vào ViewBag để truyền sang View
            ViewBag.TotalRevenue = totalRevenue;

            return View();
        }


        public ActionResult TotalRevenueChart()
        {
            var revenueByMonth = db.Orders
                .Where(o => o.Status == "Delivered")
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Month = g.Key.Year + "-" + g.Key.Month, // Format: Năm-Tháng
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(g => g.Month)
                .ToList();

            ViewBag.RevenueByMonth = revenueByMonth;

            return View();
        }



        public JsonResult GetOrderStatusData()
        {
            var orderStatusCount = db.Orders
                                     .GroupBy(o => o.Status)
                                     .Select(g => new { Status = g.Key, OrderCount = g.Count() })
                                     .ToList();

            return Json(orderStatusCount, JsonRequestBehavior.AllowGet); // Trả về dữ liệu dưới dạng JSON
        }



    }
}