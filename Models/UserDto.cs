using Newtonsoft.Json;

namespace MyFirstMvcApp.Models
{
    public class UserDto
    {
        public int UserId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }
    }
}