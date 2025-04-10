using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyFirstMvcApp.Data;
using MyFirstMvcApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SessionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves all sessions.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSessions()
        {
            var sessions = await _context.Sessions.ToListAsync();
            return Ok(sessions);
        }

        /// <summary>
        /// Retrieves a specific session by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSession(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            return Ok(session);
        }

        /// <summary>
        /// Creates a new session.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSession([FromBody] Session session, string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            session.UserId = userId;
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }

        /// <summary>
        /// Updates an existing session.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSession(int id, [FromBody] Session session)
        {
            if (id != session.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(session).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes a specific session by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            // Step 1: Get related training programs
            var trainingPrograms = await _context.TrainingPrograms
                .Where(tp => tp.SessionId == id)
                .ToListAsync();

            var trainingProgramIds = trainingPrograms.Select(tp => tp.Id).ToList();

            // Step 2: Get all trials related to those training programs
            var trialsToDelete = await _context.Trials
                .Where(t => trainingProgramIds.Contains(t.TrainingId))
                .ToListAsync();

            // Step 3: Delete the trials
            _context.Trials.RemoveRange(trialsToDelete);

            // Step 4: Delete the training programs
            _context.TrainingPrograms.RemoveRange(trainingPrograms);

            // Step 5: Delete the session itself
            _context.Sessions.Remove(session);

            // Save all changes in one transaction
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Retrieves the status of a session by ID.
        /// </summary>
        [HttpGet("status/{sessionId}")]
        public async Task<IActionResult> GetSessionStatus(int sessionId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found.");
            }

            var status = session.FinalResults.Count == session.NumberOfTrials ? "Completed" : "InProgress";
            return Ok(new { Status = status });
        }

        /// <summary>
        /// Retrieves the DPrime score of a session by ID.
        /// </summary>
        [HttpGet("dprime/{sessionId}")]
        public async Task<IActionResult> GetDPrimeScore(int sessionId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found.");
            }

            if (session.FinalResults == null || session.FinalResults.Count == 0)
            {
                return Ok(new { DPrime = 0.0 });
            }

            var hitCount = session.FinalResults.Count(result => result == "H");
            var dPrime = (double)hitCount / session.FinalResults.Count;

            session.DPrimeScore = (float)dPrime; // Update the session's DPrimeScore with the calculated value
            await _context.SaveChangesAsync();

            return Ok(new { DPrime = dPrime });
        }

        /// <summary>
        /// Retrieves all sessions by user ID.
        /// </summary>
        [HttpGet("byUserId/{userId}")]
        public async Task<IActionResult> GetSessionsByUserId(string userId)
        {
            var sessions = await _context.Sessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Date) // Order by date descending
                .ToListAsync();

            if (sessions == null || !sessions.Any())
            {
                return NotFound($"No sessions found for user with ID {userId}.");
            }
            return Ok(sessions);
        }
    }
}
