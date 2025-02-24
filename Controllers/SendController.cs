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
    public class SendController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SendController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Send
        [HttpGet]
        public async Task<IActionResult> GetSends()
        {
            var sends = await _context.Sends.ToListAsync();
            return Ok(sends);
        }

        // GET: api/Send/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSend(int id)
        {
            var send = await _context.Sends.FindAsync(id);
            if (send == null)
            {
                return NotFound();
            }
            return Ok(send);
        }

        // POST: api/Send
        [HttpPost]
        public async Task<IActionResult> AddSend([FromBody] Send send)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (send.SelectedLocation < 0 || send.SelectedLocation > 3)
            {
                return BadRequest("SelectedLocation must be between 0 and 3.");
            }

            var trainingProgram = await _context.TrainingPrograms.FindAsync(send.TrainingId);
            if (trainingProgram == null)
            {
                return BadRequest($"TrainingProgram with ID {send.TrainingId} not found.");
            }

            var session = await _context.Sessions.FindAsync(trainingProgram.SessionId);
            if (session == null)
            {
                return BadRequest($"Session with ID {trainingProgram.SessionId} not found.");
            }

            // Check for unique TrainingId
            var existingSend = await _context.Sends.FirstOrDefaultAsync(s => s.TrainingId == send.TrainingId);
            if (existingSend != null)
            {
                return BadRequest($"A send with TrainingId {send.TrainingId} already exists.");
            }

            // Calculate the Result attribute
            send.Result = CalculateResult(send.SelectedLocation, trainingProgram.PositiveLocation, trainingProgram.NegativeLocation, session.ContainerType);

            _context.Sends.Add(send);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSend), new { id = send.Id }, send);
        }

        // PUT: api/Send/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSend(int id, [FromBody] Send updatedSend)
        {
            if (id != updatedSend.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trainingProgram = await _context.TrainingPrograms.FindAsync(updatedSend.TrainingId);
            if (trainingProgram == null)
            {
                return BadRequest($"TrainingProgram with ID {updatedSend.TrainingId} not found.");
            }

            var session = await _context.Sessions.FindAsync(trainingProgram.SessionId);
            if (session == null)
            {
                return BadRequest($"Session with ID {trainingProgram.SessionId} not found.");
            }

            // Recalculate the Result attribute
            updatedSend.Result = CalculateResult(updatedSend.SelectedLocation, trainingProgram.PositiveLocation, trainingProgram.NegativeLocation, session.ContainerType);

            _context.Entry(updatedSend).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Send/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSend(int id)
        {
            var send = await _context.Sends.FindAsync(id);
            if (send == null)
            {
                return NotFound();
            }

            _context.Sends.Remove(send);
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
