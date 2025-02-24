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

        // GET: api/TrainingProgram/Random/{sessionId}
        [HttpGet("Random/{sessionId}")]
        public async Task<IActionResult> GetRandomTrainingPrograms(int sessionId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found.");
            }

            var random = new Random();
            var trainingPrograms = new List<TrainingProgram>();

            int specialSendNumber = -1;
            if (session.SendX)
            {
                specialSendNumber = random.Next(session.NumberOfSends / 2, session.NumberOfSends);
            }

            for (int i = 0; i < session.NumberOfSends; i++)
            {
                var trainingProgram = new TrainingProgram
                {
                    SessionId = sessionId,
                    SendNumber = i
                };

                if (i == specialSendNumber)
                {
                    trainingProgram.PositiveLocation = 0;
                    trainingProgram.NegativeLocation = 0;
                }
                else{
                    if (session.ContainerType == ContainerType.PositiveControl)
                    {
                        trainingProgram.NegativeLocation = 0;
                        trainingProgram.PositiveLocation = random.Next(1, 4); // 1, 2, or 3
                    }
                    else if (session.ContainerType == ContainerType.PositiveNegativeControl)
                    {
                        do
                        {
                            trainingProgram.NegativeLocation = random.Next(1, 4); // 1, 2, or 3
                            trainingProgram.PositiveLocation = random.Next(1, 4); // 1, 2, or 3
                        } while (trainingProgram.NegativeLocation == trainingProgram.PositiveLocation);
                    }
                }

                trainingPrograms.Add(trainingProgram);
            }

            return Ok(trainingPrograms);
        }


        // POST: api/TrainingProgram
        [HttpPost]
        public async Task<IActionResult> AddTrainingPrograms([FromBody] IEnumerable<TrainingProgram> programs)
        {
            if (!programs.Any())
            {
                return BadRequest("No programs provided.");
            }

            foreach (var program in programs)
            {
                if (!TryValidateModel(program))
                {
                    return BadRequest(ModelState);
                }

                var session = await _context.Sessions.FindAsync(program.SessionId);
                if (session == null)
                {
                    return BadRequest($"Session with ID {program.SessionId} not found.");
                }

                if (program.SendNumber < 0 || program.SendNumber > session.NumberOfSends)
                {
                    return BadRequest($"SendNumber for program {program.SendNumber} must be between 0 and {session.NumberOfSends}.");
                }

                if (program.PositiveLocation == program.NegativeLocation && program.PositiveLocation != 0)
                {
                    return BadRequest("PositiveLocation and NegativeLocation cannot be the same.");
                }

                // Check for duplicates
                var existingProgram = await _context.TrainingPrograms
                    .FirstOrDefaultAsync(tp => tp.SessionId == program.SessionId && tp.SendNumber == program.SendNumber);
                if (existingProgram != null)
                {
                    return BadRequest($"A training program with SessionId {program.SessionId} and SendNumber {program.SendNumber} already exists.");
                }

                // Add each program to the context
                _context.TrainingPrograms.Add(program);
            }

            // Save all programs at once
            await _context.SaveChangesAsync();

            var addedPrograms = await _context.TrainingPrograms
                .Where(tp => programs.Select(p => p.Id).Contains(tp.Id))
                .ToListAsync();

            return Ok(addedPrograms);
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