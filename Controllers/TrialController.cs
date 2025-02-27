using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyFirstMvcApp.Data;
using MyFirstMvcApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TrialController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrialController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Trial
        [HttpGet]
        public async Task<IActionResult> GetTrials()
        {
            var trials = await _context.Trials.ToListAsync();
            return Ok(trials);
        }

        // GET: api/Trial/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrial(int id)
        {
            var trial = await _context.Trials.FindAsync(id);
            if (trial == null)
            {
                return NotFound();
            }
            return Ok(trial);
        }

        // POST: api/Trial
        [HttpPost]
        public async Task<IActionResult> AddTrial([FromBody] Trial trial)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (trial.SelectedLocation < 0 || trial.SelectedLocation > 3)
            {
                return BadRequest("SelectedLocation must be between 0 and 3.");
            }

            var trainingProgram = await _context.TrainingPrograms.FindAsync(trial.TrainingId);
            if (trainingProgram == null)
            {
                return BadRequest($"TrainingProgram with ID {trial.TrainingId} not found.");
            }

            var session = await _context.Sessions.FindAsync(trainingProgram.SessionId);
            if (session == null)
            {
                return BadRequest($"Session with ID {trainingProgram.SessionId} not found.");
            }

            // Check for unique TrainingId
            var existingTrial = await _context.Trials.FirstOrDefaultAsync(s => s.TrainingId == trial.TrainingId);
            if (existingTrial != null)
            {
                return BadRequest($"A trial with TrainingId {trial.TrainingId} already exists.");
            }

            // Calculate the Result attribute
            trial.Result = CalculateResult(trial.SelectedLocation, trainingProgram.PositiveLocation, trainingProgram.NegativeLocation, session.ContainerType);

            _context.Trials.Add(trial);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrial), new { id = trial.Id }, trial);
        }

        // PUT: api/Trial/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrial(int id, [FromBody] Trial updatedTrial)
        {
            if (id != updatedTrial.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trainingProgram = await _context.TrainingPrograms.FindAsync(updatedTrial.TrainingId);
            if (trainingProgram == null)
            {
                return BadRequest($"TrainingProgram with ID {updatedTrial.TrainingId} not found.");
            }

            var session = await _context.Sessions.FindAsync(trainingProgram.SessionId);
            if (session == null)
            {
                return BadRequest($"Session with ID {trainingProgram.SessionId} not found.");
            }

            // Recalculate the Result attribute
            updatedTrial.Result = CalculateResult(updatedTrial.SelectedLocation, trainingProgram.PositiveLocation, trainingProgram.NegativeLocation, session.ContainerType);

            _context.Entry(updatedTrial).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Trial/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrial(int id)
        {
            var trial = await _context.Trials.FindAsync(id);
            if (trial == null)
            {
                return NotFound();
            }

            _context.Trials.Remove(trial);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private string CalculateResult(int selectedLocation, int positiveLocation, int negativeLocation, ContainerType containerType)
        {
            if (selectedLocation == positiveLocation)
            {
                return "H"; // Hit: Correct selection
            }
            else if (selectedLocation == negativeLocation)
            {
                // False Accept (FA): Incorrect selection of negative location (if ContainerType is 1)
                if (containerType == ContainerType.PositiveNegativeControl)
                {
                    return "FA"; // False Accept (Incorrect selection of NegativeLocation)
                }
                else
                {
                    return "M"; // Miss if NegativeLocation is selected in PositiveControl (ContainerType 0)
                }
            }
            else if (containerType == ContainerType.PositiveControl && (selectedLocation == 1 || selectedLocation == 2 || selectedLocation == 3))
            {
                return "CR"; // Correct Reject (CR): Any valid PositiveLocation (1, 2, 3) if ContainerType is 0
            }

            return "M"; // Default to Miss if none of the above conditions are met
        }
    }
}
