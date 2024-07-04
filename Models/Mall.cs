using System;
using System.ComponentModel.DataAnnotations;

namespace MallMinder.Models
{
    public class Mall
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [StringLength(255)]
        public string Location { get; set; }

        public int TotalRooms { get; set; }

        public int TotalFloors { get; set; }


        [Display(Name = "Added Date")]
        [DataType(DataType.DateTime)]
        public DateTime AddedDate { get; set; }
    }
}
