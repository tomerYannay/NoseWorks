using Microsoft.AspNetCore.Mvc;
using MyFirstMvcApp.Data;
using MyFirstMvcApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SessionController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> CreateTraining([FromBody] Session session)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
