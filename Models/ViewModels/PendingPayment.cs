using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class PendingPayment
    {
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int Room { get; set; }
        public DateTime PaymentDate { get; set; }
        public int LateMonths { get; set; }
    }
}