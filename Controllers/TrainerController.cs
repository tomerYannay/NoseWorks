using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace MyFirstMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TrainerController : ControllerBase
    {
        private static readonly string FilePath = "trainers.json";
        private static List<string> names = LoadNamesFromFile();

        // GET: api/Trainer
        [HttpGet]
        public IActionResult GetNames()
        {
            return Ok(names);
        }

        // POST: api/Trainer
        [HttpPost]
        public IActionResult AddName([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Name cannot be empty.");
            }

            if (names.Contains(name))
            {
                return BadRequest("Name already exists.");
            }

            names.Add(name);
            SaveNamesToFile(names);
            return Ok(names);
        }

        // DELETE: api/Trainer/{name}
        [HttpDelete("{name}")]
        public IActionResult DeleteName(string name)
        {
            var existingName = names.FirstOrDefault(n => n == name);
            if (existingName == null)
            {
                return NotFound($"Name '{name}' not found.");
            }

            names.Remove(existingName);
            SaveNamesToFile(names);
            return Ok(names);
        }

        private static List<string> LoadNamesFromFile()
        {
            if (!System.IO.File.Exists(FilePath))
            {
                return new List<string>();
            }

            var json = System.IO.File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }

        private static void SaveNamesToFile(List<string> names)
        {
            var json = JsonConvert.SerializeObject(names);
            System.IO.File.WriteAllText(FilePath, json);
        }
    }
}