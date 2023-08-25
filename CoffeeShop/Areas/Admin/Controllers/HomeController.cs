using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoffeeShop.DB;
using CoffeeShop.Models;

namespace CoffeeShop.Areas.Admin.Controllers
{
    
    public class HomeController : Controller
    {
        CoffeeShopEntities db = new CoffeeShopEntities();

        [HttpGet]
        public ActionResult LoginPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoginPage(string mail, string password)
        {   
            var user = db.Users.FirstOrDefault(x => x.mail == mail && x.password == password);
            /* Admin mi?*/
            if (user.isadmin == 1)//(user.Id == 1)
            {
                Session["OnlineAdmin"] = user;
                Session["UserId"]=user.Id;
                //Session["OnlineUser"] = user;
                return RedirectToAction("SaveBasket");

            }
            /* kullanı mi?*//*
            else if (user.isadmin == 0)
            {
                Session["OnlineUser"] = user;
                return RedirectToAction("Index"); //RedirectToAction("SaveBasket");
            }*/

            else
            {
                return View();
            }

        }
        public ActionResult Logout()
        {/*
            Session["OnlineAdmin"] = null;
            Session["OnlineUser"] = null;
            Session["temporderid"] = null;*/
            Session.RemoveAll();
            return RedirectToAction("LoginPage");

        }
        // GET: Admin/Home
        public ActionResult Index()
        {
            if (Session["OnlineAdmin"] == null)
            {
                return RedirectToAction("LoginPage");
            }
            var coffees = db.coffees.ToList();

            return View(coffees);
        }

       
        int totalPrice = 0;
        public ActionResult SaveBasket()
        {
            if (Session["OnlineAdmin"] == null)
            {
                return RedirectToAction("LoginPage");
            }

            int userid = Convert.ToInt32(Session["UserId"]);
            var orders = db.Orders
                .Where(a => a.order_status == 1)
                .FirstOrDefault();
            if(orders != null)
            {
                var orderdetails = db.OrderDetail.Where(a => a.OrderId == orders.Id).ToList();

                //var basket = db.Basket.Where(x => x.IpAddress == Request.UserHostAddress).ToList();
                List<coffees> coffeeList = new List<coffees>(); // kahve tablosunun data sprinti 
                Dictionary<int, int> quantityDictionary = new Dictionary<int, int>();
                BasketModel basketModel = new BasketModel();
                basketModel.CoffeeModel = new List<coffees>();
                //basketModel.SepetModel = new List<Basket>();
                basketModel.OrderDetailsModel = new List<OrderDetail>();

                foreach (var item in orderdetails)//basket)
                {
                    var Coffee = db.coffees.FirstOrDefault(x => x.Id == item.OrderCoffeeId);
                    basketModel.CoffeeModel.Add(Coffee);
                    //basketModel.SepetModel.Add(item);
                    basketModel.OrderDetailsModel.Add(item);
                    coffeeList.Add(Coffee);
                    int Coffeeprice = Convert.ToInt32(Coffee.CoffeePrice);
                    int count = Convert.ToInt32(item.amount);


                    totalPrice = totalPrice + Coffeeprice * count;
                    ViewBag.tp = totalPrice;

                }
                return View(basketModel);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult CompleteOrder()
        {
            //int userid = Convert.ToInt32(Session["UserId"]);
            //var update = db.Orders.Where(x => x.order_status == 1).FirstOrDefault();
            //var update = db.Orders.Find(id);
            var update = db.Orders.FirstOrDefault(x => x.order_status == 1);

            if (update != null)
            {
                //db.Basket.Remove(delete);
                update.order_status = 0;
                db.SaveChanges();
                return RedirectToAction("SaveBasket");
            }


            return View(); 
        }

        //[HttpPost]
        //public ActionResult SaveBasket(string name, string address, int totalprice)
        //{
        //    if (Session["OnlineAdmin"] == null)
        //    {
        //        return RedirectToAction("LoginPage");
        //    }

        //    Orders order = new Orders();
        //    order.OrderCustomerFullName = name;
        //    order.OrderAddress = address;
        //    order.OrderDate = DateTime.Now;
        //    order.OrderTotalPrice = totalprice;
        //    db.Orders.Add(order);
        //    db.SaveChanges();
        //    int lastInsertedId = order.Id;
        //    var basket = db.Basket.Where(x => x.IpAddress == Request.UserHostAddress).ToList();
        //    foreach (var item in basket)
        //    {
        //        OrderDetail detail = new OrderDetail();
        //        detail.OrderId = lastInsertedId;
        //        detail.OrderCoffeeId = item.CoffeeId;
        //        db.OrderDetail.Add(detail);
        //        db.Basket.Remove(item);
        //        db.SaveChanges();
        //    }

        //    return RedirectToAction("Orders");
        //    //return View();
        //}

        public ActionResult DeleteKahve(int id)
        {
            if (Session["OnlineAdmin"] == null)
            {
                return RedirectToAction("LoginPage");
            }

            //var delete = db.Basket.FirstOrDefault(x => x.Id == id);//basket tablosunda id li kahve var mı?
            //var delete = db.OrderDetail.FirstOrDefault(x=>x.OrderCoffeeId == id);
            var delete = db.OrderDetail.Where(x => x.OrderId == id).FirstOrDefault();
            var deleteorderitem = db.OrderDetail.Find(id);

            if (delete != null)
            {
                //db.Basket.Remove(delete);
                db.OrderDetail.Remove(delete);
                db.SaveChanges();
                return RedirectToAction("SaveBasket");
            }
            else
            {
                return RedirectToAction("Admin");
            }

        }
        public ActionResult AddCoffee()
        {
            if (Session["OnlineAdmin"] == null)
            {
                return RedirectToAction("LoginPage");
            }
            return View();
        }
        [HttpPost]
        public ActionResult AddCoffee(string name, int price, string description, string type, string image)
        {
            if (Session["OnlineAdmin"] == null)
            {
                return RedirectToAction("LoginPage");
            }
            coffees addCoffee = new coffees();
            addCoffee.CoffeeName = name;
            addCoffee.CoffeePrice = price;
            addCoffee.CoffeeDescription = description;
            if (type == "1")
            {
                addCoffee.CoffeeType = true;
            }
            else
            {
                addCoffee.CoffeeType = false;
            }

            if (Request.Files != null && Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file.ContentLength > 0)
                {
                    var folder = Server.MapPath("~/images/");
                    var filename = Guid.NewGuid() + ".jpg";
                    file.SaveAs(Path.Combine(folder, filename));
                    var filePath = "images/" + filename;
                    addCoffee.CoffeeImagePath = filePath;
                }
            }
            db.coffees.Add(addCoffee);
            db.SaveChanges();
            return RedirectToAction("AddCoffee");
        }
        public ActionResult UpdateCoffee(int id)
        {
            if (Session["OnlineAdmin"] == null)
            {
                return RedirectToAction("LoginPage");
            }
            var updateCoffee = db.coffees.FirstOrDefault(x => x.Id == id);
            return View(updateCoffee);
        }

        [HttpPost]
        public ActionResult UpdateCoffee(string name, int price, string description, string type, string image, int id = 0)
        {
            if (Session["OnlineAdmin"] == null)
            {
                return RedirectToAction("LoginPage");
            }

            if (id == 0)
            {
                return RedirectToAction("Admin");
            }

            var update = db.coffees.FirstOrDefault(x => x.Id == id);
            update.CoffeeName = name;
            update.CoffeePrice = price;
            update.CoffeeDescription = description;
            if (type == "1")
            {
                update.CoffeeType = true;
            }
            else
            {
                update.CoffeeType = false;
            }

            if (Request.Files != null && Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file.ContentLength > 0)
                {
                    var folder = Server.MapPath("~/images/");
                    var filename = Guid.NewGuid() + ".jpg";
                    file.SaveAs(Path.Combine(folder, filename));
                    var filePath = "images/" + filename;
                    update.CoffeeImagePath = filePath;
                }
            }
            db.SaveChanges();

            return RedirectToAction("Admin");
        }
    }
}