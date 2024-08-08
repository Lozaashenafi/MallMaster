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

        [Required(ErrorMessage = "Rental Date is required")]
        [DataType(DataType.Date)]
        public DateTime RentalDate { get; set; }

        [Required(ErrorMessage = "Payment Duration is required")]
        [Range(1, 120, ErrorMessage = "Payment Duration must be between 1 and 120 months")]
        public int PaymentDuration { get; set; }

        public int? TypeId { get; set; }

        [StringLength(100, ErrorMessage = "Other type description cannot be longer than 100 characters")]
        public string? Other { get; set; }

        // Constructor to set default values
        public RentVM()
        {
            RentalDate = DateTime.Now; // Initialize RentalDate with current date
        }
    }
}
