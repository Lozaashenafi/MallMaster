using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models
{
    public class TenantPayment
    {
        public int Id { get; set; }
        public int RentId { get; set; }
        public double? Price { get; set; }
        public string CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaidDate { get; set; }
        public double? ReferenceNumber { get; set; }
        public virtual Rent Rent { get; set; }

    }
}