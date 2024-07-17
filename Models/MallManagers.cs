using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models
{
    public class MallManagers
    {
        public int Id { get; set; }
        public int MallId { get; set; }
        public bool IsActive { get; set; } = true;
        public String? OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public virtual AppUser ownerId { get; set; }

    }
}