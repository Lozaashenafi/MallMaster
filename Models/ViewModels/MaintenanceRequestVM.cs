using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class MaintenanceRequestVM
    {
        public int Id { get; set; }
        public string TenantName { get; set; }
        public string MaintenanceType { get; set; }
    }
}