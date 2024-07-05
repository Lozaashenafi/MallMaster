using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models
{
    public class Rent
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string TenantId { get; set; }
        public string CreatedBy { get; set; }
        public int TypeId { get; set; }
        public int PaymentDuration { get; set; }

        public DateTime RentalDate { get; set; }
        public DateTime AddedDate { get; set; }
    }
}