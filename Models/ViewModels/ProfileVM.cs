using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMaster.Models.ViewModels
{
    public class ProfileVM
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ImageUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MallName { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public bool? IsActive { get; set; }
    }
}