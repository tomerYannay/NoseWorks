using System.ComponentModel.DataAnnotations;

namespace MyFirstMvcApp.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}