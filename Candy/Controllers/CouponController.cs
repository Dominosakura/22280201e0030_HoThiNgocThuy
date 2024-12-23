using Candy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Candy.Controllers
{
    public class CouponController : Controller
    {
        DECOMMERCEEntities3  db = new DECOMMERCEEntities3();
        // GET: Coupon
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetCouponDetails(int id)
        {
            return View();
        }

        [HttpGet]
        public ActionResult Create()
        {


            return View();
        }
    }
}