using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Models;

namespace MallMinder.Models.ViewModels
{
    public class PriceVM
    {
        public int FloorNumber { get; set; }
        public float PricePerCare { get; set; }
        public int RoomNumber { get; set; }
        public float RoomPrice { get; set; }
    }
}
