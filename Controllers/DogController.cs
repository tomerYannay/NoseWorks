using Microsoft.AspNetCore.Mvc;
using MyFirstMvcApp.Data;
using MyFirstMvcApp.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DogController(ApplicationDbContext context)
        {
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
        public async Task<IActionResult> CreateDog([FromBody] Dog dog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
    }
}
