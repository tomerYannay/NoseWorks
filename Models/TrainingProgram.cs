using System;
using System.ComponentModel.DataAnnotations;

namespace MyFirstMvcApp.Models{

    public class TrainingProgram{
        [Required]
        public int Id { get; set; }  // מזהה

        [Required]
        public int SendNumber { get; set; }  // מספר שליחות

        [Required]
        public int PositiveLocation { get; set; }  // מיקום חיובי (1-3)

        [Required]
        public int NegativeLocation { get; set; }  // מיקום שלילי (1-3)

        [Required]
        public virtual ICollection<Session> Sessions { get; set; }  // קשר לאימונים
    }
    
}