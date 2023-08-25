using CoffeeShop.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoffeeShop.Models
{
    public class BasketModel
    {

        public List<coffees> CoffeeModel { get; set; }
        public List<Basket> SepetModel { get; set; }
        public List<OrderDetail> OrderDetailsModel { get; set; }

    }
}