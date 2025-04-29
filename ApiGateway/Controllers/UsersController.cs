using ApiGateway.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// use this address http://localhost:5182/api/Users to use the controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]

    /// <summary>
    /// Controller for managing user-related operations such as registration, login, deletion, and API capability introspection.
    /// </summary>
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// A simple in-memory list to simulate a database of users.
        /// </summary>
        private static List<User> users = new List<User> {
            new() { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", Password = "password123" },
            new() { Id = Guid.NewGuid(), Name = "Jane Smith", Email = "jane@example.com", Password = "password456" }
        };

        /// <summary>
        /// Returns all available HTTP methods supported by the Gateway server for this controller.
        /// </summary>
        /// <returns>200 OK with an Allow header listing supported methods.</returns>
        [HttpOptions("options")]
        public IActionResult GetUsersOptions()
        {
            Response.Headers.Append("Allow", "GET, POST, PUT, DELETE, OPTIONS");
            return Ok();
        }


        /// <summary>
        /// Registers a new user to the Gateway server.
        /// </summary>
        /// <param name="username">The username of the new user.</param>
        /// <param name="email">The email address of the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <returns>
        /// 200 OK with the registered user object if input is valid;
        /// 400 Bad Request if email or password is missing.
        /// </returns>
        [HttpPut("register")]
        public IActionResult Register([FromForm] string username, [FromForm] string email, [FromForm] string password)
        {
            var client = new HttpClient();

            var newUser = new User { Id = Guid.NewGuid(), Email = email, Name = username, Password = password, apiKey = Guid.NewGuid() };
            Console.WriteLine("Received put req");

            if (newUser.Email != null && newUser.Password != null)
            {
                users.Add(newUser);
                return Ok(newUser);
            }

            return BadRequest("Enter a valid email address");
        }

        /// <summary>
        /// Authenticates a user to the Gateway server.
        /// </summary>
        /// <param name="username">The username of the user (optional if email is provided).</param>
        /// <param name="email">The email address of the user (optional if username is provided).</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>
        /// 200 OK with a welcome message if credentials are valid;
        /// 401 Unauthorized if credentials are invalid.
        /// </returns>
        [HttpPost("login")]
        public IActionResult Login([FromForm] string username, [FromForm] string email, [FromForm] string password)
        {
            Console.WriteLine("Received post req");

            var user = users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                var helloUser = user.Name != null ? user.Name : user.Email;
                return Ok($"Welcome {helloUser}");
            }

            return Unauthorized("Invalid credentials");

            // Set login session
            // HttpContext.Session.SetString("UserId", user.Id.ToString());
            // HttpContext.Session.SetString("APIKey", user.apiKey.ToString());
        }

        /// <summary>
        /// Deletes a user from the in-memory user list based on the provided email address.
        /// </summary>
        /// <param name="email">The email of the user to delete.</param>
        /// <returns>
        /// 200 OK if the user was found and deleted;
        /// 404 Not Found if no user with the specified email exists.
        /// </returns>
        [HttpDelete("delete")]
        public IActionResult DeleteUser([FromQuery] string email)
        {
            var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                users.Remove(user);
                return Ok($"User with email '{email}' was deleted.");
            }

            return NotFound("User not found");
        }

    }
}
