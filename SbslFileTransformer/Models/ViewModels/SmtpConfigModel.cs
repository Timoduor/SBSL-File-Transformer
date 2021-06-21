using System.ComponentModel.DataAnnotations;

namespace SbslFileTransformer.Models
{
    public class SmtpConfigModel
    {
        [Required] public string Name { get; set; }

        [Required] public string EmailAddress { get; set; }

        [Required] public string SmtpServer { get; set; }

        [Required] public int Port { get; set; }

        [Required] public string UserName { get; set; }

        [Required] public string Password { get; set; }

        public string Recipients { get; set; }
        public bool UseSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }
    }
}