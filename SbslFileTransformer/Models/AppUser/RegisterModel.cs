using System.ComponentModel.DataAnnotations;

namespace SbslFileTransformer.Models
{
    public class RegisterModel
    {
        [StringLength(50, MinimumLength = 2)]
        public string Username { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
    }
}