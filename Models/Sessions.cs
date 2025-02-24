using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MyFirstMvcApp.Models
{

    public class Session
    {
        [Key]
        public int Id { get; set; }

        public int DogId { get; set; }

        public string Trainer { get; set; }
        
        public DateTime Date { get; set; }

        [Required]
        public int NumberOfSends { get; set; } = 10;

        [Required]
        public ContainerType ContainerType { get; set; }
        
        public bool SendX { get; set; }

        public List<string> FinalResults { get; set; } = new List<string>();

        public float DPrimeScore { get; set; }
    }
}