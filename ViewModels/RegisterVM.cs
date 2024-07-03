using System;
using System.ComponentModel.DataAnnotations;

namespace MallMinder.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "username is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords don't match. ")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public DateTime AddedDate { get; set; } // AddedDate property of type DateTime

        // Constructor to set default values
        public RegisterVM()
        {
            IsActive = true; // Set default value for IsActive
            AddedDate = DateTime.Now; // Initialize AddedDate with current date and time
        }

        // Property for IsActive
        public bool IsActive { get; set; }
    }
}
