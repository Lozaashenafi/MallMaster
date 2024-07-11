using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MallMinder.Models
{
    public class PricePerCare
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int? FloorId { get; set; }
        public int MallId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public virtual Floor Floor { get; set; }
        public virtual Mall Mall { get; set; }
        [ForeignKey("CreatedBy")]
        public virtual AppUser createdBy { get; set; }
    }
}