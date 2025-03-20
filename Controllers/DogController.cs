using Microsoft.AspNetCore.Mvc;
using MyFirstMvcApp.Data;
using MyFirstMvcApp.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;

namespace MyFirstMvcApp.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAmazonS3 _s3Client;

        public DogController(IAmazonS3 s3Client, ApplicationDbContext context)
        {
            _s3Client = s3Client;
            _context = context;
        }

        // GET: api/Dog
        [HttpGet]
        public async Task<IActionResult> GetDogs()
        {
            var dogs = await _context.Dogs.ToListAsync();
            return Ok(dogs);
        }

        // GET: api/Dog/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDog(int id)
        {
            var dog = await _context.Dogs.FindAsync(id);
            if (dog == null)
            {
                return NotFound();
            }
            return Ok(dog);
        }

        // POST: api/Dog
        [HttpPost]
        public async Task<IActionResult> CreateDog([FromBody] Dog dog, string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            dog.UserId = userId;
            _context.Dogs.Add(dog);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDog), new { id = dog.Id }, dog);
        }

        // PUT: api/Dog/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDog(int id, [FromBody] Dog dog)
        {
            if (id != dog.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(dog).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Dog/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDog(int id)
        {
            var dog = await _context.Dogs.FindAsync(id);
            if (dog == null)
            {
                return NotFound();
            }

            _context.Dogs.Remove(dog);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("uploadImage/{dogId}")]
        public async Task<IActionResult> UploadImage(int dogId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var dog = await _context.Dogs.FindAsync(dogId);
            if (dog == null)
            {
                return NotFound($"Dog with ID {dogId} not found.");
            }

            // Define the bucket name and key (file name) for the image
            var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME");
            var keyName = $"dogs/{dogId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            try
            {
                using (var newMemoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(newMemoryStream);

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

                // Store the URL of the uploaded image in the dog model
                dog.ImageUrl = $"https://{bucketName}.s3.amazonaws.com/{keyName}";

                // Update the Dog entity with the new image URL
                _context.Dogs.Update(dog);
                await _context.SaveChangesAsync();

                return Ok(new { FileName = keyName, FilePath = dog.ImageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error uploading file: {ex.Message}");
            }
        }

        [HttpGet("getImage/{dogId}")]
        public async Task<IActionResult> GetImage(int dogId)
        {
            // Find the dog by its ID
            var dog = await _context.Dogs.FindAsync(dogId);
            if (dog == null)
            {
                return NotFound($"Dog with ID {dogId} not found.");
            }

            // Check if the dog has an image URL
            if (string.IsNullOrEmpty(dog.ImageUrl))
            {
                return NotFound("No image found for this dog.");
            }
            // Return the image URL in the response
            return Ok(new { ImageUrl = dog.ImageUrl });
        }

        /// <summary>
        /// Retrieves all dogs by user ID.
        /// </summary>
        [HttpGet("byUserId/{userId}")]
        public async Task<IActionResult> GetDogsByUserId(string userId)
        {
            var dogs = await _context.Dogs
                .Where(s => s.UserId == userId)
                .ToListAsync();

            if (dogs == null || !dogs.Any())
            {
                return NotFound($"No dogs found for user with ID {userId}.");
            }
            return Ok(dogs);
        }

        [HttpGet("analysis/{dogId}")]
        public async Task<IActionResult> GetDogAnalysis(int dogId)
        {
            // Retrieve all sessions related to the given dogId
            var sessions = await _context.Sessions
                .Where(s => s.DogId == dogId)
                .ToListAsync();

            if (sessions == null || !sessions.Any())
            {
                return NotFound($"No sessions found for Dog ID {dogId}.");
            }

            // Extract list of DPrime scores
            var dprimes = sessions.Select(s => s.DPrimeScore).ToList();

            // Initialize counters
            int hitCount = 0;
            int missCount = 0;
            int totalTrials = 0;

            foreach (var session in sessions)
            {
                if (session.FinalResults != null)
                {
                    hitCount += session.FinalResults.Count(result => result == "H");
                    missCount += session.FinalResults.Count(result => result == "M");
                }
                totalTrials += session.NumberOfTrials; // Sum up the total trials from all sessions
            }

            // Return the analysis as JSON
            return Ok(new
            {
                DogId = dogId,
                NumberOfSessions = sessions.Count, // Total number of sessions
                TotalTrials = totalTrials, // Total number of trials across all sessions
                DPrimes = dprimes,
                HitCount = hitCount,
                MissCount = missCount
            });
        }


    }
}
