
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MallMinder.Models
{
    public class AppUser : IdentityUser
    {

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? InMall { get; set; }
        public bool? IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? AddedDate { get; set; } = DateTime.Now;
        [ForeignKey("InMall")]
        public virtual Mall Mall { get; set; }
    }
}