using System;
using System.ComponentModel.DataAnnotations;

namespace MyFirstMvcApp.Models{

    public class Send{
        [Key]
        public int Id {get; set; }
        
        [Required]
        public int TrainingId {get; set; }

        [Required]
        public int SelectedLocation { get; set; }  // מיקום נבחר

        [Required]
        public string Result { get; set; }

        [Required]
        public List<int> Visits { get; set; }

        [Required]
        public List<string> Results { get; set; }

        [Required]
        public virtual Session Session { get; set; }
    }
    
}