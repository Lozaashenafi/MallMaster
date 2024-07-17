using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Models.ViewModels;

namespace MallMinder.Models.ViewModels
{
    public class MaintenanceVM
    {
        public int? RentId { get; set; }
        public int? MaintenanceTypeId { get; set; }
        public string? Other { get; set; }
        public DateTime RequestedDate { get; set; }
        public int MaintenanceId { get; set; }
        public double Cost { get; set; }
        public DateTime CompletedDate { get; set; } = DateTime.Now;
    }
}