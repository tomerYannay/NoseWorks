using Microsoft.AspNetCore.Mvc;
using MyFirstMvcApp.Models;
using System;
using System.Collections.Generic;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingProgramController : ControllerBase
    {
        private static List<TrainingProgram> trainingPrograms = new List<TrainingProgram>
        {
            new TrainingProgram { Id = 1, SendNumber = 5, PositiveLocation = 1, NegativeLocation = 3 },
            new TrainingProgram { Id = 2, SendNumber = 3, PositiveLocation = 2, NegativeLocation = 1 }
        };

        /// <summary>
        /// Retrieves all training programs.
        /// </summary>
        [HttpGet]
        public IActionResult GetTrainingPrograms()
        {
            return Ok(trainingPrograms);
        }

        [HttpGet("{id}")]
        public IActionResult GetTrainingProgram(int id)
        {
            var program = trainingPrograms.Find(tp => tp.Id == id);
            if (program == null)
            {
                return NotFound();
            }
            return Ok(program);
        }

        [HttpPost]
        public IActionResult AddTrainingProgram([FromBody] TrainingProgram program)
        {
            trainingPrograms.Add(program);
            return CreatedAtAction(nameof(GetTrainingProgram), new { id = program.Id }, program);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTrainingProgram(int id, [FromBody] TrainingProgram updatedProgram)
        {
            var program = trainingPrograms.Find(tp => tp.Id == id);
            if (program == null)
            {
                return NotFound();
            }
            program.SendNumber = updatedProgram.SendNumber;
            program.PositiveLocation = updatedProgram.PositiveLocation;
            program.NegativeLocation = updatedProgram.NegativeLocation;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTrainingProgram(int id)
        {
            var program = trainingPrograms.Find(tp => tp.Id == id);
            if (program == null)
            {
                return NotFound();
            }
            trainingPrograms.Remove(program);
            return NoContent();
        }
    }
}
