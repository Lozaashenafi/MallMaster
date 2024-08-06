using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class PaymentSummery
    {
        public string TenantName { get; set; }
        public DateTime RentalDate { get; set; }
        public int RoomNumber { get; set; }
        public string PaymentStatus { get; set; }
        public int UnpaidRounds { get; set; }
        public double? PaymentAmount { get; set; }
    }
}