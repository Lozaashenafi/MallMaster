using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class TenantMaintenanceRequestVM
    {
        public int TypeId { get; set; }
        public string? Other { get; set; }
        public DateTime Date { get; set; }
    }
}