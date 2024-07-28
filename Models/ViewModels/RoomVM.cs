using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class RoomVM
    {
        [Required(ErrorMessage = "Floor Number is required")]
        public int FloorId { get; set; }
        [Required(ErrorMessage = "Care is required")]
        public int Care { get; set; }
        [Required(ErrorMessage = "RoomNumber is required")]
        public int RoomNumber { get; set; }
        public string? Description { get; set; }

    }
}