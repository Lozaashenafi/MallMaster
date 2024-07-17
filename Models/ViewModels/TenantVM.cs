using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class TenantVM
    {
        public int? RentId { get; set; }
        public string TenantName { get; set; }
        public string? TenantId { get; set; }
        public int? RoomNumber { get; set; }
        public string? FloorNumber { get; set; }
        public string TenantPhone { get; set; }
        public string? RentType { get; set; }
    }
}