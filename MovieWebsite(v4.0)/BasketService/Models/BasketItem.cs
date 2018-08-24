﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasketService.Models
{
    public class BasketItem
    {
        //correspond to movieId
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string CompanyName { get; set; }
        public int Quantity { get; set; }

    }
}