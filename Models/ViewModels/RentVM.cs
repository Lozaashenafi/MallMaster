using System;
using System.ComponentModel.DataAnnotations;

namespace MallMinder.Models.ViewModels
{
    public class RentVM
    {
        [Required(ErrorMessage = "User Name is required")]
        public string TenantId { get; set; }

        [Required(ErrorMessage = "Room is required")]
        public int RoomId { get; set; }

        public DateTime RentalDate { get; set; }
        public int PaymentDuration { get; set; }
        public int? TypeId { get; set; }
        public string? Other { get; set; }

    }
}
