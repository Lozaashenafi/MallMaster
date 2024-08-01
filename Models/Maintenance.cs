using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Import the correct namespace for Identity


namespace MallMinder.Models
{
    public class Maintenance
    {
        public int Id { get; set; }
        public int? MaintenanceTypeId { get; set; }

        public int? RentId { get; set; }
        public int? MallId { get; set; }

        public DateTime? RequestedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Description { get; set; }
        public double? Cost { get; set; }

        public virtual Rent Rent { get; set; }

        public virtual Mall Mall { get; set; }
        public virtual MaintenanceType MaintenanceType { get; set; }

    }
}