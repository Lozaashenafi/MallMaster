using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class TenantDetailVM
    {
        public string TenantName { get; set; }
        public int RentId { get; set; }
        public int Room { get; set; }
        public string Floor { get; set; }
        public string Type { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime RentedDate { get; set; }

    }
}