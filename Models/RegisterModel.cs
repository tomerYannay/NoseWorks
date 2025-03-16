using System.ComponentModel.DataAnnotations;

namespace MyFirstMvcApp.Models
{
    public class RegisterModel
    {
        [Key]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Password { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; }
    }
}