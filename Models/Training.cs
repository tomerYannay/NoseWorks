using System;
using System.ComponentModel.DataAnnotations;


namespace MyFirstMvcApp.Models{

    public class Training
    {
        [Required]
        public int Id { get; set; }  // מזהה
        [Required]
        public int DogId { get; set; }  // מזהה כלב
        [Required]
        public virtual Dog Dog { get; set; }  // קשר לאובייקט כלב (מפתח זר)
        [Required]
        public string Trainer { get; set; }  // מאמן
        [Required]
        public DateTime Date { get; set; }  // תאריך
        [Required]
        public int NumberOfSends { get; set; }  // מספר שליחות
        [Required]
        public string ContainerType { get; set; }  // סוגי מכולות (positive-control, positive-negative-control)
        
        [Required]
        // שליחה X: ערך בוליאני (למשל שליחה מס' 1, 2, 3)
        public bool SendX { get; set; }

        [Required]
        public int TrainingProgramId { get; set; }  // מזהה תוכנית אימון

        [Required]
        public virtual TrainingProgram TrainingProgram { get; set; }  // קשר לתוכנית אימון

        // תוצאה סופית
        [Required]
        public List<string> FinalResults { get; set; }  // רשימה של תוצאות (H, FA, CR)

        // D-prime score
        [Required]
        public float DPrimeScore { get; set; }  // D-prime score (float)
    }
        
}