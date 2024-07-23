using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class PaymentVM
    {
        public string TenantName { get; set; }
        public string TenantPhone { get; set; }
        public string? PaymentStatus { get; set; }
        public int RentId { get; set; }
        public DateTime Date { get; set; }
        public double ReferenceNumber { get; set; }

    }
}