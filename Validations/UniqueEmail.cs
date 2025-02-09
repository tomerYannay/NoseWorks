using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyFirstMvcApp.Data;

namespace MyFirstMvcApp.Validations
{
    public class UniqueEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || !(value is string email))
                return ValidationResult.Success;  // or return an error if null is not valid

            var context = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext));
            if (context.Users.Any(u => u.Email == email))
            {
                return new ValidationResult("Email already in use.");
            }
            return ValidationResult.Success;
        }
    }
}
