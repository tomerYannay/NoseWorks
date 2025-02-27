using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFirstMvcApp.Data;
using MyFirstMvcApp.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoUploadController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ApplicationDbContext _context;

        public VideoUploadController(IAmazonS3 s3Client, ApplicationDbContext context)
        {
            _s3Client = s3Client;
            _context = context;
        }

        // POST: api/VideoUpload/{trainingProgramId}
        [HttpPost("{trainingProgramId}")]
        public async Task<IActionResult> UploadVideo(int trainingProgramId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var trainingProgram = await _context.TrainingPrograms.FindAsync(trainingProgramId);
            if (trainingProgram == null)
            {
                return NotFound($"TrainingProgram with ID {trainingProgramId} not found.");
            }

            var session = await _context.Sessions.FindAsync(trainingProgram.SessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {trainingProgram.SessionId} not found.");
            }

            var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME");
            var keyName = $"{session.Id}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

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

                trainingProgram.VideoUrl = $"https://{bucketName}.s3.amazonaws.com/{keyName}";
                _context.TrainingPrograms.Update(trainingProgram);
                await _context.SaveChangesAsync();

                return Ok(new { FileName = keyName, FilePath = trainingProgram.VideoUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error uploading file: {ex.Message}");
            }
        }

        // GET: api/VideoUpload/{trainingProgramId}
        [HttpGet("{trainingProgramId}")]
        public async Task<IActionResult> GetVideo(int trainingProgramId)
        {
            var trainingProgram = await _context.TrainingPrograms.FindAsync(trainingProgramId);
            if (trainingProgram == null)
            {
                return NotFound($"TrainingProgram with ID {trainingProgramId} not found.");
            }

            var videoUrl = trainingProgram.VideoUrl;
            if (string.IsNullOrEmpty(videoUrl))
            {
                return NotFound("No video found for this training program.");
            }

            return Ok(new { Url = videoUrl });
        }
    }
}