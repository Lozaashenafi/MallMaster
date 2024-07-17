using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models
{
    public class Floor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FloorNumber { get; set; }

        [Required]
        public int MallId { get; set; }
        public bool IsActive { get; set; } = true;


        public virtual Mall Mall { get; set; }
    }
}