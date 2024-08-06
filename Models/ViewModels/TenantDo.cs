using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMaster.Models.ViewModels
{
    public class TenantDo
    {

        public int Id { get; set; }
        public DateTime RentalDate { get; set; }
        public string Name { get; set; }
        public int RoomNumber { get; set; }
        public int FloorId { get; set; }
        public int PaymentDuration { get; set; }
        public double Care { get; set; }
        public bool PricePercareFlag { get; set; }
        public int RoomId { get; set; }

    }
}