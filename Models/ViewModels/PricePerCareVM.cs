using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Models;

namespace MallMinder.Models.ViewModels
{
    public class PricePerCareVM
    {
        public int? FloorNumber { get; set; }
        public decimal Price { get; set; }
    }
}