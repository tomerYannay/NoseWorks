using Microsoft.AspNetCore.Mvc;
using MyFirstMvcApp.Models;
using System;
using System.Collections.Generic;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingController : ControllerBase
    {
        private static List<Training> trainings = new List<Training>
        {
            new Training { Id = 1, DogId = 1, Trainer = "John Doe", Date = new DateTime(2023, 3, 15), NumberOfSends = 5, ContainerType = "positive-control", SendX = true, TrainingProgramId = 1, FinalResults = new List<string> { "H", "H", "FA", "H" }, DPrimeScore = 1.5f },
            new Training { Id = 2, DogId = 2, Trainer = "Jane Smith", Date = new DateTime(2023, 4, 10), NumberOfSends = 3, ContainerType = "positive-negative-control", SendX = true, TrainingProgramId = 2, FinalResults = new List<string> { "H", "CR", "H" }, DPrimeScore = 2.1f }
        };

        /// <summary>
        /// Retrieves all trainings.
        /// </summary>
        [HttpGet]
        public IActionResult GetTrainings()
        {
            return Ok(trainings);
        }

        [HttpGet("{id}")]
        public IActionResult GetTraining(int id)
        {
            var training = trainings.Find(t => t.Id == id);
            if (training == null)
            {
                return NotFound();
            }
            return Ok(training);
        }

        [HttpPost]
        public IActionResult AddTraining([FromBody] Training training)
        {
            trainings.Add(training);
            return CreatedAtAction(nameof(GetTraining), new { id = training.Id }, training);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTraining(int id, [FromBody] Training updatedTraining)
        {
            var training = trainings.Find(t => t.Id == id);
            if (training == null)
            {
                return NotFound();
            }
            training.DogId = updatedTraining.DogId;
            training.Trainer = updatedTraining.Trainer;
            training.Date = updatedTraining.Date;
            training.NumberOfSends = updatedTraining.NumberOfSends;
            training.ContainerType = updatedTraining.ContainerType;
            training.SendX = updatedTraining.SendX;
            training.TrainingProgramId = updatedTraining.TrainingProgramId;
            training.FinalResults = updatedTraining.FinalResults;
            training.DPrimeScore = updatedTraining.DPrimeScore;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTraining(int id)
        {
            var training = trainings.Find(t => t.Id == id);
            if (training == null)
            {
                return NotFound();
            }
            trainings.Remove(training);
            return NoContent();
        }
    }
}
