using System;
using System.ComponentModel.DataAnnotations;


namespace MyFirstMvcApp.Models{

        public class Session
    {
        public int Id { get; set; }
        public int DogId { get; set; }
        public string Trainer { get; set; }
        public DateTime Date { get; set; }
        public int NumberOfSends { get; set; }
        public string ContainerType { get; set; }
        public bool SendX { get; set; }
        public int TrainingProgramId { get; set; }
        public List<string> FinalResults { get; set; }
        public float DPrimeScore { get; set; }
    }
        
}