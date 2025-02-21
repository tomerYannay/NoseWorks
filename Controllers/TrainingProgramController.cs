using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFirstMvcApp.Data;
using MyFirstMvcApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Apply authorization to the entire controller
    public class TrainingProgramController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainingProgramController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TrainingProgram
        [HttpGet]
        public async Task<IActionResult> GetTrainingPrograms()
        {
            var trainingPrograms = await _context.TrainingPrograms.Include(tp => tp.SessionId).ToListAsync();
            return Ok(trainingPrograms);
        }

        // GET: api/TrainingProgram/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrainingProgram(int id)
        {
            var trainingProgram = await _context.TrainingPrograms.FirstOrDefaultAsync(tp => tp.Id == id);
            if (trainingProgram == null)
            {
                return NotFound();
            }
            return Ok(trainingProgram);
        }

        // POST: api/TrainingProgram
        [HttpPost]
        public async Task<IActionResult> AddTrainingProgram([FromBody] TrainingProgram program)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var session = await _context.Sessions.FindAsync(program.SessionId);
            if (session == null)
            {
                return BadRequest("Session not found.");
            }

            if (program.SendNumber < 0 || program.SendNumber > session.NumberOfSends)
            {
                return BadRequest($"SendNumber must be between 0 and {session.NumberOfSends}.");
            }

            _context.TrainingPrograms.Add(program);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTrainingProgram), new { id = program.Id }, program);
        }

        // PUT: api/TrainingProgram/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrainingProgram(int id, [FromBody] TrainingProgram updatedProgram)
        {
            if (id != updatedProgram.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var session = await _context.Sessions.FindAsync(updatedProgram.SessionId);
            if (session == null)
            {
                return BadRequest("Session not found.");
            }

            if (updatedProgram.SendNumber < 0 || updatedProgram.SendNumber > session.NumberOfSends)
            {
                return BadRequest($"SendNumber must be between 0 and {session.NumberOfSends}.");
            }

            _context.Entry(updatedProgram).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/TrainingProgram/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrainingProgram(int id)
        {
            var trainingProgram = await _context.TrainingPrograms.FindAsync(id);
            if (trainingProgram == null)
            {
                return NotFound();
            }

            _context.TrainingPrograms.Remove(trainingProgram);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}