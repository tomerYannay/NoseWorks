using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyFirstMvcApp.Data;
using MyFirstMvcApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TrialController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAmazonS3 _s3Client;

        public TrialController(IAmazonS3 s3Client, ApplicationDbContext context)
        {
            _s3Client = s3Client;
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

            if (trial.VisitedLocations != null && trial.VisitedLocations.Any(location => location < 0 || location > 3))
            {
                return BadRequest("Each value in VisitedLocations must be between 0 and 3.");
            }

            // Calculate the Result attribute
            trial.Result = CalculateResult(trial.SelectedLocation, trainingProgram.PositiveLocation, trainingProgram.NegativeLocation, session.ContainerType);
            _context.Trials.Add(trial);

            await _context.SaveChangesAsync();
            
            await UpdateFinalResultsForSession(trial.TrainingId);

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

            if (updatedTrial.SelectedLocation < 0 || updatedTrial.SelectedLocation > 3)
            {
                return BadRequest("SelectedLocation must be between 0 and 3.");
            }

            if (updatedTrial.VisitedLocations != null && updatedTrial.VisitedLocations.Any(location => location < 0 || location > 3))
            {
                return BadRequest("Each value in VisitedLocations must be between 0 and 3.");
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

            // Recalculate the Result attribute for the updated trial
            updatedTrial.Result = CalculateResult(updatedTrial.SelectedLocation, trainingProgram.PositiveLocation, trainingProgram.NegativeLocation, session.ContainerType);

            _context.Entry(updatedTrial).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await UpdateFinalResultsForSession(updatedTrial.TrainingId);

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
            await UpdateFinalResultsForSession(trial.TrainingId);
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

        // POST: api/Trial/uploadVideo/{trialId}
        [HttpPost("uploadVideo/{trialId}")]
        public async Task<IActionResult> UploadVideo(int trialId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var trial = await _context.Trials.FindAsync(trialId);
            if (trial == null)
            {
                return NotFound($"Trial with ID {trialId} not found.");
            }

            var bucketName = "noseworks";
            var keyName = $"{trialId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            try
            {
                using (var newMemoryStream = new MemoryStream())
                {
                    file.CopyTo(newMemoryStream);

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = keyName,
                        BucketName = bucketName,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    var fileTransferUtility = new TransferUtility(_s3Client);
                    await fileTransferUtility.UploadAsync(uploadRequest);
                }

                trial.VideoUrl = $"https://{bucketName}.s3.amazonaws.com/{keyName}";
                _context.Trials.Update(trial);
                await _context.SaveChangesAsync();

                return Ok(new { FileName = keyName, FilePath = trial.VideoUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error uploading file: {ex.Message}");
            }
        }

        // GET: api/Trial/getVideo/{trialId}
        [HttpGet("getVideo/{trialId}")]
        public async Task<IActionResult> GetVideo(int trialId)
        {
            var trial = await _context.Trials.FindAsync(trialId);
            if (trial == null)
            {
                return NotFound($"Trial with ID {trialId} not found.");
            }

            var videoUrl = trial.VideoUrl;
            if (string.IsNullOrEmpty(videoUrl))
            {
                return NotFound("No video found for this trial.");
            }

            return Ok(new { Url = videoUrl });
        }

        private async Task UpdateFinalResultsForSession(int trialTrainingId)
        {
            // Get the training program associated with the trial
            var trainingProgram = await _context.TrainingPrograms
                .FirstOrDefaultAsync(tp => tp.Id == trialTrainingId);

            if (trainingProgram == null)
            {
                throw new ArgumentException("TrainingProgram not found.");
            }

            // Get the session associated with the training program
            var session = await _context.Sessions.FindAsync(trainingProgram.SessionId);
            if (session == null)
            {
                throw new ArgumentException("Session not found.");
            }

            // Fetch all trials related to the same sessionId by joining TrainingProgram to Trial
            var trials = await _context.Trials
                .Where(t => _context.TrainingPrograms
                    .Any(tp => tp.SessionId == session.Id && tp.Id == t.TrainingId))  // Filter by SessionId
                .ToListAsync();

            // Update the FinalResults list with all trial results
            session.FinalResults = trials.Select(t => t.Result).ToList();

            // Update the session with the new FinalResults
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();
        }

        // GET: api/Trial/bySession/{sessionId}
        [HttpGet("bySession/{sessionId}")]
        public async Task<IActionResult> GetTrialsBySession(int sessionId)
        {
            var trainingPrograms = await _context.TrainingPrograms
                .Where(tp => tp.SessionId == sessionId)
                .ToListAsync();

            if (trainingPrograms == null || !trainingPrograms.Any())
            {
                return NotFound($"No training programs found for session with ID {sessionId}.");
            }

            var trainingProgramIds = trainingPrograms.Select(tp => tp.Id).ToList();

            var trials = await _context.Trials
                .Where(t => trainingProgramIds.Contains(t.TrainingId))
                .ToListAsync();

            return Ok(trials);
        }
    }
}