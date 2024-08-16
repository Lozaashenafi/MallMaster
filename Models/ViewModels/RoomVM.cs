using System.ComponentModel.DataAnnotations;

namespace MallMinder.Models.ViewModels
{
    public class RoomVM
    {
        [Required(ErrorMessage = "Floor Number is required")]
        public int FloorId { get; set; }

        [Required(ErrorMessage = "Care is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Care must be a positive number")]
        public int Care { get; set; }

        [Required(ErrorMessage = "Room Number is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Room Number must be a positive number")]
        public int RoomNumber { get; set; }

        public string? Description { get; set; }
    }
}
