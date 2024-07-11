using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MallMinder.Models
{
    public class Rent
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string TenantId { get; set; }
        public string CreatedBy { get; set; }
        public int? TypeId { get; set; }
        public int PaymentDuration { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime AddedDate { get; set; }

        // Navigation properties
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        [ForeignKey("TenantId")]
        public virtual AppUser Tenant { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual AppUser CreatedByUser { get; set; }

        [ForeignKey("TypeId")]
        public virtual RentType RentType { get; set; }
    }
}
