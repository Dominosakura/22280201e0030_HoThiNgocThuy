using Candy.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Candy.Controllers
{
    public class BuyerController : Controller
    {
        DECOMMERCEEntities3 db = new DECOMMERCEEntities3();

        private ActionResult IsBuyer()
        {
         
            var user = Session["User"] as User;

            
            if (user == null || user.IdRole != 1 || user.Status != true)
            {
                return RedirectToAction("DangNhap", "User");
            }

            
            return null;
        }

        public ActionResult Index()
        {
           
            var result = IsBuyer();
            if (result != null)
            {
                return result; 
            }

            return View();
        }

        public ActionResult AddToFavorites(int productID)
        {

            var result = IsBuyer();
            if (result != null) return result;

            var buyer  = (User)Session["User"];
            var existingFavorite = db.Favorites.SingleOrDefault(f => f.UserID == buyer.UserID && f.ProductID == productID);
            if (existingFavorite != null)
            {
                return RedirectToAction("ShowFavorites");
            }

            var favorite = new Favorite
            {
                UserID = buyer.UserID,
                ProductID = productID,
                AddedDate = DateTime.Now
            };

            try
            {
                db.Favorites.Add(favorite);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while adding to favorites.");
                return View("ShowFavorites");
            }

            return RedirectToAction("ShowFavorites");
        }

        public ActionResult ShowFavorites()
        {

            var result = IsBuyer();
            if (result != null) return result;

            var buyer = (User)Session["User"];
            var favoriteProducts = db.Favorites
                .Where(f => f.UserID == buyer.UserID)
                .Select(f => new
                {
                    Product = f.Product,
                    MainImageURL = f.Product.ProductImages
                        .Where(pi => pi.IsMainImage == true)
                        .Select(pi => pi.ImageURL)
                        .FirstOrDefault()
                }).ToList();

            var viewModel = favoriteProducts.Select(fp => new ProductImage
            {
                ProductID = fp.Product.ProductID,
                ImageURL = fp.MainImageURL,
                Product = fp.Product
            }).ToList();

            return View(viewModel);
        }

        public ActionResult RemoveFromFavorites(int? productID)
        {
            var result = IsBuyer();
            if (result != null) return result;

            var buyer = (User)Session["User"];

            var existingFavorite = db.Favorites
                .SingleOrDefault(f => f.UserID == buyer.UserID && f.ProductID == productID);

            if (existingFavorite != null)
            {
                db.Favorites.Remove(existingFavorite);
                db.SaveChanges();
            }
            else
            {
                ViewBag.Message = "Product was not in favorites.";
            }

            return RedirectToAction("ShowFavorites");

        }

        public ActionResult MyOrders()
        {

          
            var result = IsBuyer();
            if (result != null) return result;

            var buyer = (User)Session["User"];

            int buyerId = buyer.UserID; 

            ViewBag.PendingConfirmationCount = db.Orders
                .Where(o => o.BuyerID == buyerId && o.Status == "Pending Confirmation")
                .Count();

            ViewBag.AwaitingPickupCount = db.Orders
                .Where(o => o.BuyerID == buyerId && o.Status == "Awaiting Pickup")
                .Count();

            ViewBag.AwaitingDeliveryCount = db.Orders
                .Where(o => o.BuyerID == buyerId && o.Status == "Awaiting Delivery")
                .Count();

            ViewBag.DeliveredCount = db.Orders
                .Where(o => o.BuyerID == buyerId && o.Status == "Delivered")
                .Count();

            ViewBag.ReviewedCount = db.Orders
                .Where(o => o.BuyerID == buyerId && o.Status == "Reviewed")
                .Count();

            ViewBag.CancelledCount = db.Orders
                .Where(o => o.BuyerID == buyerId && o.Status == "Cancelled")
                .Count();

            return View();
        }


    }
}
