using System;
using System.ComponentModel.DataAnnotations;

namespace SbslFileTransformer.Models.AppUser
{
    public class LoginModel
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [EmailAddress]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username should be 3 to 50 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(40, MinimumLength = 6, ErrorMessage = "Password should be 6 to 40 characters")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}