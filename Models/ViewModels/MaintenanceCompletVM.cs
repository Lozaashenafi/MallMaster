using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Models;

namespace MallMinder.Models.ViewModels
{
    public class MaintenanceCompletVM
    {
        public int MaintenanceId { get; set; }
        public float Cost { get; set; }
        public DateTime CompletedDate { get; set; } = DateTime.Now;
    }
}