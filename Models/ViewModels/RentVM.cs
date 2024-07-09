using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class RentVM
    {
        [Required(ErrorMessage = "User Name  is required")]
        public string TenantUserName { get; set; }
        [Required(ErrorMessage = "Room is required")]
        public int RoomId { get; set; }
        public DateTime RentalDate { get; set; }
        public int? PaymentDuration { get; set; }
        public string StatusType { get; set; }
        public string StatusTag { get; set; }
        public string? Other { get; set; }
    }
}