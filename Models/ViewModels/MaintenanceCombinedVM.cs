using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Models.ViewModels;

namespace MallMinder.Models.ViewModels
{
    public class MaintenanceCombinedVM
    {
        public MaintenanceRequestVM RequestVM { get; set; }
        public MaintenanceCompletVM CompletVM { get; set; }
    }
}