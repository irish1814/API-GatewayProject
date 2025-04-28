using ApiGateway.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    // http://localhost:5182/api/Users
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // A simple in-memory list to simulate a database
        private static List<User> users = new List<User> {
            new() { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Password = "password123" },
            new() { Id = Guid.NewGuid(), Name = "Jane Smith", Email = "jane@example.com", Password = "password456" }
        };

        [HttpGet("ListAll")]
        public IActionResult GetUsers()
        {
            return Ok(users);
        }

        [HttpGet("ApiKey")]
        public IActionResult GenerateApiKey()
        {
            return Ok(Guid.NewGuid().ToString());
        }

        [HttpPost("login")]
        public IActionResult PostLogin([FromForm] string username, [FromForm] string password)
        {
            var client = new HttpClient();

            var formData = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            };
            Console.WriteLine("Received post req");

            if (formData["username"] == "admin" && formData["password"] == "1234")
            {
                return Ok(Guid.NewGuid().ToString());
            }

            return Unauthorized(new { Message = "Wrong username or password" });
        }
    }
}
