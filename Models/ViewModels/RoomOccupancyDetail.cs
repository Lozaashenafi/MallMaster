using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMaster.Models.ViewModels
{
    public class RoomOccupancyDetail
    {
        public int RoomNumber { get; set; }
        public string FloorNumber { get; set; }
        public double OccupancyPercentage { get; set; }
    }
}