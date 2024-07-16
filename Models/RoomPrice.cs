using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Models;

namespace MallMinder.Models
{
    public class RoomPrice
    {
        public int Id { get; set; }
        public float Price { get; set; }
        public int RoomId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public virtual Room Room { get; set; }
        [ForeignKey("CreatedBy")]
        public virtual AppUser createdBy { get; }
    }
}