
using System.ComponentModel.DataAnnotations;


namespace MallMinder.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "UserName is required.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

    }
}