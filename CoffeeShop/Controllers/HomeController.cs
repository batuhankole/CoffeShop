using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using CoffeeShop.DB;
using CoffeeShop.Models;

namespace CoffeeShop.Controllers
{
    public class HomeController : Controller
    {

        CoffeeShopEntities db = new CoffeeShopEntities();
        int temporderid = 0;
        public ActionResult Index()
        {

            var coffeeList = db.coffees.Take(6).ToList();
            return View(coffeeList);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Service()
        {
            return View();
        }

        public ActionResult Menu()
        {
            var coffeeList = db.coffees.ToList();
            Random rand = new Random();
            Session["temporderid"] = rand.Next(1, 1000);


            return View(coffeeList);
        }

        [HttpPost]
        public ActionResult Menu(int id=0, int id2=0, string Sayi="", string Sayi2="")
        {

            Basket sepet = new Basket();

            sepet.IpAddress = Request.UserHostAddress;
            sepet.temporderid = Convert.ToInt32(Session["temporderid"]);

            if (Sayi != "" && id > 0)
            {
                sepet.CoffeeId = id;
                sepet.number = Convert.ToInt32(Sayi);
            }
            else if (Sayi2 != "" && id2 > 0)
            {
                sepet.CoffeeId = id2;
                sepet.number = Convert.ToInt32(Sayi2);
            }

            db.Basket.Add(sepet);
            var saving = db.SaveChanges();
            if (saving > 0)
            {
                return RedirectToAction("SaveBasket");
            }
            else
            {
                return View();
            }

        }

        public ActionResult Contact() 
        {
            
            return View();
        }
      

        public ActionResult DeleteCoffee(int id)
        {
            if (Session["OnlineUser"] == null)
            {
                return RedirectToAction("LoginPage");
            }
            var delete = db.coffees.FirstOrDefault(x => x.Id == id);//coffees tablosunda id li kahve var mı?
            if (delete != null)
            {
                db.coffees.Remove(delete);
                db.SaveChanges();
                return RedirectToAction("Admin");
            }
            else
            {
                return RedirectToAction("Admin");
            }

        }
        public ActionResult DeleteKahve(int id)
        {
            if (Session["OnlineUser"] == null)
            {
                return RedirectToAction("LoginPage");
            }

            var delete = db.Basket.FirstOrDefault(x => x.Id == id);//basket tablosunda id li kahve var mı?
            if (delete != null)
            {
                db.Basket.Remove(delete);
                db.SaveChanges();
                return RedirectToAction("SaveBasket");
            }
            else
            {
                return RedirectToAction("Admin");
            }

        }
        
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string adSoyad, string mail, string sifre)
        {
           
            Users user = new Users();
            user.Fullname = adSoyad;
            user.mail = mail;
            user.password = sifre;
            user.isadmin = 0;
           
            db.Users.Add(user);
            db.SaveChanges();

            return RedirectToAction("LoginPage");
        }
        [HttpGet]
        public ActionResult LoginPage()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoginPage(string mail, string password)
        {
            var user = db.Users.FirstOrDefault(x => x.mail == mail && x.password == password);
            /* Admin mi?*//*
            if (user.isadmin == 1)//(user.Id == 1)
            {
                Session["OnlineAdmin"] = user;
                //Session["OnlineUser"] = user;
                return RedirectToAction("SaveBasket");

            }*/
            /* kullanı mi?*/
            if (user.isadmin == 0)
            {
                Session["OnlineUser"] = user;
                Session["UserId"] = user.Id;
                return RedirectToAction("Index"); //RedirectToAction("SaveBasket");
            }

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
        
    

        int totalPrice = 0;
        public ActionResult SaveBasket()
        {
            if (Session["OnlineUser"] == null)
            {
                return RedirectToAction("LoginPage");
            }
            //var basket = db.Basket.Where(x => x.IpAddress == Request.UserHostAddress).ToList();
            int temporderid = Convert.ToInt32(Session["temporderid"]);
            var basket = db.Basket.Where(x => x.temporderid == temporderid).ToList();
            List<coffees> coffeeList = new List<coffees>();
            Dictionary<int, int> quantityDictionary = new Dictionary<int, int>();
            BasketModel basketModel = new BasketModel();
            basketModel.CoffeeModel = new List<coffees>();
            basketModel.SepetModel = new List<Basket>();
          

            foreach (var item in basket)
            {
                var Coffee = db.coffees.FirstOrDefault(x => x.Id == item.CoffeeId);
                basketModel.CoffeeModel.Add(Coffee);
                basketModel.SepetModel.Add(item);
                coffeeList.Add(Coffee);
                int Coffeeprice = Convert.ToInt32(Coffee.CoffeePrice);
                int count = Convert.ToInt32(item.number);
     
     
                totalPrice = totalPrice + Coffeeprice * count;
                ViewBag.tp = totalPrice;
    

            }


           
            return View(basketModel);
        }

        [HttpPost]
        public ActionResult SaveBasket(string name, string address, int totalprice)
        {
            if (Session["OnlineUser"] == null)
            {
                return RedirectToAction("LoginPage");
            }
            Orders order = new Orders();

            order.OrderCustomerFullName = name;
            order.OrderAddress = address;
            order.OrderDate = DateTime.Today;
            order.OrderTotalPrice = totalprice;
            order.Userid = Convert.ToInt32(Session["UserId"]);
            order.order_status = 1;
            db.Orders.Add(order);
            db.SaveChanges();
            int lastInsertedId = order.Id;

            //Basket basket = new Basket();
            //basket.orderid = lastInsertedId;

            int torderid = Convert.ToInt32(Session["temporderid"]);
            var basket = db.Basket.Where(x => x.temporderid == torderid).ToList();
            foreach (var item in basket)
            {
                OrderDetail detail = new OrderDetail();
                detail.OrderId = lastInsertedId;
                detail.OrderCoffeeId = item.CoffeeId;
                detail.amount = item.number;
                db.OrderDetail.Add(detail);
                db.Basket.Remove(item);
                db.SaveChanges();
            }
            Session.Remove("temporderid");

            return RedirectToAction("Orders");
            //return View();
        }

        public ActionResult Orders()
        {
            int userid = Convert.ToInt32(Session["UserId"]);
            var Orders = db.Orders.Where(a => a.Userid == userid).ToList();
            List<coffees> mylist = new List<coffees>();
           
            foreach (var item in Orders)
            {
                var detail = db.OrderDetail.Where(x => x.OrderId == item.Id).ToList();
                foreach (var detay in detail)
                {
                    var names = db.coffees.FirstOrDefault(x => x.Id == detay.OrderCoffeeId);
                    mylist.Add(names);
                }
        
        
            }
            ViewBag.list = mylist;
            return View(Orders);
        }

    

    }
}