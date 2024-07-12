using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models
{
    public class MaintenanceStatus
    {
        public int Id { get; set; }
        public int MaintenanceId { get; set; }
        public DateTime Date { get; set; }
        public String? CreatedBy { get; set; }
        public int StatusId { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual Maintenance Maintenance { get; set; }
        [ForeignKey("StatusId")]
        public virtual MaintenanceStatusType statusId { get; set; }
        [ForeignKey("CreatedBy")]
        public virtual AppUser createdBy { get; set; }


    }
}