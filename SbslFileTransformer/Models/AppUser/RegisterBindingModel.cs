using System.ComponentModel.DataAnnotations;

namespace SbslFileTransformer.Models
{
    public class RegisterBindingModel
    {
        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string LastName { get; set; }

        public string IdNumber { get; set; }
        public string Address { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string Organization { get; set; }
    }
}