using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Models;

namespace MallMinder.Models
{
    public class RoomOccupancies
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public DateTime OccupiedDate { get; set; }
        public DateTime? ReleasedDate { get; set; }
        public virtual Room Room { get; set; }
    }
}