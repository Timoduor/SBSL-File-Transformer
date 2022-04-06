using System.ComponentModel.DataAnnotations;

namespace SbslFileTransformer.Models.AppUser
{
    public class RemoveLoginBindingModel
    {
        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Login provider")]
        public string LoginProvider { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Provider key")]
        public string ProviderKey { get; set; }
    }
}