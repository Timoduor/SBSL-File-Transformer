using System.ComponentModel.DataAnnotations;

namespace SbslFileTransformer.Models
{
    // Models used as parameters to AccountController actions.

    public class AddExternalLoginBindingModel
    {
        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "External access token")]
        public string ExternalAccessToken { get; set; }
    }
}