using System;
using System.ComponentModel.DataAnnotations;

namespace MallMinder.Models.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
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
