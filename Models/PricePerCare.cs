using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MallMinder.Models
{
    public class PricePerCare
    {
        public int Id { get; set; }
        public double? Price { get; set; }
        public int? FloorId { get; set; }
        public int? MallId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        [ForeignKey("FloorId")]
        public virtual Floor Floor { get; set; }
        [ForeignKey("MallId")]
        public virtual Mall Mall { get; set; }
        [ForeignKey("CreatedBy")]
        public virtual AppUser createdBy { get; set; }
    }
}