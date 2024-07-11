using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class MaintenanceRequestVM
    {
        public int RentId { get; set; }
        public int MaintenanceTypeId { get; set; }
        public string? Other { get; set; }
        public DateTime RequestedDate { get; set; }
    }
}