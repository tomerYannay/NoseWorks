using System;
using System.ComponentModel.DataAnnotations;

namespace MyFirstMvcApp.Models{

    public class Trial{
        [Key]
        public int Id {get; set; }
        
        [Required]
        public int TrainingId {get; set; }

        [Required]
        public int SelectedLocation { get; set; }  

        public string TargetScent { get; set; }

        [Required]
        public string Result { get; set; }
    }
}