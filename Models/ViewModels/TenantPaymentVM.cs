using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class TenantPaymentVM
    {
        public int RentId { get; set; }
        public double? Price { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaidDate { get; set; }
        public double ReferenceNumber { get; set; }
    }
}