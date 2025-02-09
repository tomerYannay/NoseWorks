using Microsoft.AspNetCore.Mvc;
using MyFirstMvcApp.Models;
using System;
using System.Collections.Generic;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DogController : ControllerBase
    {
        private static List<Dog> dogs = new List<Dog>
        {
            new Dog { Id = 1, Name = "Buddy", Breed = "Golden Retriever", DateOfBirth = new DateTime(2018, 1, 1) },
            new Dog { Id = 2, Name = "Charlie", Breed = "Labrador", DateOfBirth = new DateTime(2019, 5, 21) }
        };


        /// <summary>
        /// Retrieves all dogs.
        /// </summary>
        [HttpGet]
        public IActionResult GetDogs()
        {
            return Ok(dogs);
        }

        [HttpGet("{id}")]
        public IActionResult GetDog(int id)
        {
            var dog = dogs.Find(d => d.Id == id);
            if (dog == null)
            {
                return NotFound();
            }
            return Ok(dog);
        }

        [HttpPost]
        public IActionResult AddDog([FromBody] Dog dog)
        {
            dogs.Add(dog);
            return CreatedAtAction(nameof(GetDog), new { id = dog.Id }, dog);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateDog(int id, [FromBody] Dog updatedDog)
        {
            var dog = dogs.Find(d => d.Id == id);
            if (dog == null)
            {
                return NotFound();
            }
            dog.Id = updatedDog.Id;
            dog.Name = updatedDog.Name;
            dog.Breed = updatedDog.Breed;
            dog.DateOfBirth = updatedDog.DateOfBirth;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteDog(int id)
        {
            var dog = dogs.Find(d => d.Id == id);
            if (dog == null)
            {
                return NotFound();
            }
            dogs.Remove(dog);
            return NoContent();
        }
    }
}
