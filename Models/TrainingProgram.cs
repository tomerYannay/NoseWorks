using System;
using System.ComponentModel.DataAnnotations;

namespace MyFirstMvcApp.Models{

    public class TrainingProgram{
        [Key]
        public int Id { get; set; }  // מזהה

        [Required]
        public int TrialNumber { get; set; }  // מספר שליחה

        [Required]
        [Range(0, 3, ErrorMessage = "PositiveLocation must be between 0 and 3.")]
        public int PositiveLocation { get; set; }  // מיקום חיובי (0-3)

        [Required]
        [Range(0, 3, ErrorMessage = "NegativeLocation must be between 0 and 3.")]
        public int NegativeLocation { get; set; }  // מיקום שלילי (0-3)

        [Required]
        public int SessionId { get; set; }

    }
    
}