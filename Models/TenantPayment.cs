using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models
{
    public class TenantPayment
    {
        public int Id { get; set; }
        public int MallId { get; set; }
        public string CreatedBy { get; set; }

        public DateTime PaymentDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public double ReferenceNumber { get; set; }

    }
}