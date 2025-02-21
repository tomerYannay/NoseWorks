using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyFirstMvcApp.Models;
using System;
using System.Collections.Generic;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SendController : ControllerBase
    {
        private static List<Send> sends = new List<Send>
        {
            new Send { Id = 1, TrainingId = 1, SelectedLocation = 1, Result = "H", Visits = new List<int> { 1, 3, 2 }, Results = new List<string> { "H", "FA", "H" } },
            new Send { Id = 2, TrainingId = 2, SelectedLocation = 2, Result = "CR", Visits = new List<int> { 2, 1, 3 }, Results = new List<string> { "H", "CR", "CR" } }
        };

        /// <summary>
        /// Retrieves all sends.
        /// </summary>
        [HttpGet]
        public IActionResult GetSends()
        {
            return Ok(sends);
        }

        [HttpGet("{id}")]
        public IActionResult GetSend(int id)
        {
            var send = sends.Find(s => s.Id == id);
            if (send == null)
            {
                return NotFound();
            }
            return Ok(send);
        }

        [HttpPost]
        public IActionResult AddSend([FromBody] Send send)
        {
            sends.Add(send);
            return CreatedAtAction(nameof(GetSend), new { id = send.Id }, send);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSend(int id, [FromBody] Send updatedSend)
        {
            var send = sends.Find(s => s.Id == id);
            if (send == null)
            {
                return NotFound();
            }
            send.TrainingId = updatedSend.TrainingId;
            send.SelectedLocation = updatedSend.SelectedLocation;
            send.Result = updatedSend.Result;
            send.Visits = updatedSend.Visits;
            send.Results = updatedSend.Results;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSend(int id)
        {
            var send = sends.Find(s => s.Id == id);
            if (send == null)
            {
                return NotFound();
            }
            sends.Remove(send);
            return NoContent();
        }
    }
}
