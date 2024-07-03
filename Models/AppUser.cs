
using Microsoft.AspNetCore.Identity;

namespace MallMinder.Models
{
    public class AppUser : IdentityUser
    {

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? AddedDate { get; set; } = DateTime.Now;
    }
}