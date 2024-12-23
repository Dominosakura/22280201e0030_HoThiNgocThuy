using Candy.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Candy.Controllers
{
    public class CartController : Controller
    {
        DECOMMERCEEntities3 db = new DECOMMERCEEntities3();

        // GET: Cart
        public ActionResult Index()
        {
            return View();
        }
        private ActionResult CheckUserAccess()
        {
            var user = Session["User"] as User;
            if (user == null || user.IdRole != 1 || user.Status != true)
            {
                return RedirectToAction("DangNhap", "User");
            }
            return null;
        }

        private string GetCurrentUsername()
        {
            var user = Session["User"] as User;
            return user?.Username;
        }


        private List<ShoppingCartItem> LayGioHang()
        {
            string username = GetCurrentUsername();
            var cart = db.ShoppingCarts
                .Include("ShoppingCartItems.Product")
                .SingleOrDefault(c => c.User.Username == username);

            return cart?.ShoppingCartItems.ToList() ?? new List<ShoppingCartItem>();
        }

        public ActionResult ThemGioHang(int productID, string returnUrl)
        {
            var redirectResult = CheckUserAccess();
            if (redirectResult != null) return redirectResult;

            var product = db.Products.Find(productID);
            if (product == null)
            {
                ModelState.AddModelError("", "Sản phẩm không tồn tại.");
                return Redirect(returnUrl);
            }

            string username = GetCurrentUsername();
            var cart = db.ShoppingCarts
                .FirstOrDefault(c => c.User.Username == username) ?? CreateNewCart(username);

            AddOrUpdateCartItem(cart.CartID, productID);

            return Redirect(returnUrl);
        }

        private ShoppingCart CreateNewCart(string username)
        {
            var cart = new ShoppingCart
            {
                UserID = db.Users.SingleOrDefault(u => u.Username == username)?.UserID,
                CreatedDate = DateTime.Now
            };
            db.ShoppingCarts.Add(cart);
            db.SaveChanges();
            return cart;
        }

        private int TongSoLuong() => LayGioHang().Sum(i => i.Quantity);

        private decimal TongTien() => LayGioHang().Sum(i => i.Quantity * i.Product.Price ?? 0);

        public  void AddOrUpdateCartItem(int cartId, int productId)
        {
            var cartItem = db.ShoppingCartItems
                .SingleOrDefault(i => i.CartID == cartId && i.ProductID == productId);
            if (cartItem == null)
            {
                cartItem = new ShoppingCartItem
                {
                    CartID = cartId,
                    ProductID = productId,
                    Quantity = 1
                };
                db.ShoppingCartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity++;
            }

            db.SaveChanges();
        }

        public ActionResult GioHang()
        {
            var redirectResult = CheckUserAccess();
            if (redirectResult != null) return redirectResult;

            var items = LayGioHang();
            if (items.Count == 0)
            {
                return RedirectToAction("Index", "Product");
            }
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();

            foreach (var item in items)
            {
                item.ProductImage = item.Product.ProductImages.FirstOrDefault(img => img.IsMainImage == true);
            }

            return View(items);
        }

        public ActionResult GioHangPartial()
        {
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();

            return PartialView();
        }

        public ActionResult CapNhatGioHang(int productID, int quantity)
        {
            
            var redirectResult = CheckUserAccess();
            if (redirectResult != null) return redirectResult;

            string username = GetCurrentUsername();
            var cart = db.ShoppingCarts.SingleOrDefault(c => c.User.Username == username);

            if (cart != null)
            {
                var cartItem = db.ShoppingCartItems
                    .SingleOrDefault(i => i.CartID == cart.CartID && i.ProductID == productID);

                if (cartItem != null)
                {
                    if (quantity > 0) 
                    {
                        cartItem.Quantity = quantity;
                        db.SaveChanges();
                    }
                }
            }

            return RedirectToAction("GioHang");
        }


        public ActionResult XoaSPKhoiGioHang(int productID)
        {
            var redirectResult = CheckUserAccess();
            if (redirectResult != null) return redirectResult;

            string username = GetCurrentUsername();
            var cart = db.ShoppingCarts.SingleOrDefault(c => c.User.Username == username);

            if (cart != null)
            {
                var cartItem = db.ShoppingCartItems
                    .SingleOrDefault(i => i.CartID == cart.CartID && i.ProductID == productID);
                if (cartItem != null)
                {
                    db.ShoppingCartItems.Remove(cartItem);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("GioHang");
        }

        public ActionResult XoaGioHang()
        {
            var redirectResult = CheckUserAccess();
            if (redirectResult != null) return redirectResult;

            string username = GetCurrentUsername();
            var cart = db.ShoppingCarts.SingleOrDefault(c => c.User.Username == username);

            if (cart != null)
            {
                var items = db.ShoppingCartItems.Where(i => i.CartID == cart.CartID).ToList();
                db.ShoppingCartItems.RemoveRange(items);
                db.SaveChanges();
            }

            return RedirectToAction("GioHang");
        }

        [HttpGet]
        public ActionResult Order()
        {
            var redirectResult = CheckUserAccess();
            if (redirectResult != null) return redirectResult;

            var items = LayGioHang();
            int totalQuantity = TongSoLuong(); 
            decimal totalAmount = TongTien(); 

            string username = GetCurrentUsername();
            var buyer = db.Users.SingleOrDefault(u => u.Username == username);

            var orderModel = new Order
            {
                TotalAmount = totalAmount
            };

            ViewBag.TotalQuantity = totalQuantity;
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItems = items;
            ViewBag.Buyer = buyer;

            return View(orderModel);
        }


        public ActionResult Order(string shippingAddress, string paymentMethod, decimal? paymentAmount, string couponCode = null)
        {
            var redirectResult = CheckUserAccess();
            if (redirectResult != null) return redirectResult;

            string username = GetCurrentUsername();
            var buyer = db.Users.SingleOrDefault(u => u.Username == username);

            var cartItems = LayGioHang(); 
            if (!cartItems.Any())
            {
                ModelState.AddModelError("", "Giỏ hàng của bạn trống.");
                return RedirectToAction("GioHang");
            }

            decimal totalAmount = TongTien();
            decimal finalPaymentAmount = paymentAmount ?? totalAmount;
            ViewBag.FinalPaymentAmount = finalPaymentAmount;

       
            var order = new Order
            {
                BuyerID = buyer.UserID,
                RecipientName = buyer.FullName,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                ShippingAddress = shippingAddress,
                Status = "Pending Confirmation"
            };
            UpdateUserAndOrders(buyer.UserID, buyer.FullName, shippingAddress, buyer.Email);
            db.Orders.Add(order);
            db.SaveChanges();

          
            foreach (var item in cartItems)
            {
                var product = db.Products.SingleOrDefault(p => p.ProductID == item.ProductID);
                if (product == null)
                {
                    
                    continue;
                }

               
                if (product.StockQuantity >= item.Quantity)
                {
                    product.StockQuantity -= item.Quantity;
                }
                else
                {
                    ModelState.AddModelError("", $"Sản phẩm {product.ProductName} không đủ số lượng trong kho.");
                    return RedirectToAction("GioHang"); 
                }

                var orderDetail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price ?? 0
                };
                db.OrderDetails.Add(orderDetail);
            }
            db.SaveChanges();

            // Lưu thông tin thanh toán
            var payment = new Payment
            {
                OrderID = order.OrderID,
                PaymentDate = DateTime.Now,
                PaymentMethod = paymentMethod,
                PaymentAmount = finalPaymentAmount
            };
            db.Payments.Add(payment);
            db.SaveChanges();

          
            XoaGioHang();

           
            TempData["SuccessMessage"] = "Bạn đã đặt hàng thành công!";

          
            return RedirectToAction("OrderSuccess");
        }


        public ActionResult OrderSuccess()
        {
           
            var successMessage = TempData["SuccessMessage"] as string;

            return View((object)successMessage);
        }



        public void UpdateUserAndOrders(int userId, string newFullName, string newAddress, string newEmail)
        {
            var user = db.Users.SingleOrDefault(u => u.UserID == userId);
            if (user != null)
            {
                user.FullName = newFullName;
                user.Address = newAddress;
                user.Email = newEmail; 
                db.SaveChanges();

                var orders = db.Orders.Where(o => o.BuyerID == userId).ToList();
                foreach (var order in orders)
                {
                    order.RecipientName = newFullName; 
                    order.ShippingAddress = newAddress; 
                }

                db.SaveChanges();
            }
        }


    }
}
