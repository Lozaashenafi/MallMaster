using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models
{
    public class Room
    {
        public int Id { get; set; }
        public int RoomNumber { get; set; }

        public int FloorId { get; set; }

        public int Care { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime AddedDate { get; set; }
        public bool PricePercareFlag { get; set; }
        public bool RoomDeactivateFlag { get; set; }

        public virtual Floor Floor { get; set; }
    }
}