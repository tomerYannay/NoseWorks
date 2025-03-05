using System;
using System.ComponentModel.DataAnnotations;

namespace MyFirstMvcApp.Models{

    public class Dog{
        [Key]
        public int Id {get; set; }
        [Required]
        public string Name {get; set; }
        [Required]
        public string Breed {get; set; }
        [Required]
        public DateTime DateOfBirth {get; set; }

        public string ImageUrl { get; set; }

    }
    
}