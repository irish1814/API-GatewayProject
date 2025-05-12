using ApiGateway.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controller for managing user authentication, registration, and deletion in the Crypto Gateway.
    /// Access this controller using http://hostip:5182/api/Users/{Operation}
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly CryptoDbContext _db;
        private readonly IDistributedCache _redisCache;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class with the specified database context and Redis cache.
        /// </summary>
        /// <param name="db">The database context used for user and account data.</param>
        /// <param name="distributedCache">The Redis distributed cache used for session management and user caching.</param>
        public UsersController(CryptoDbContext db, IDistributedCache distributedCache)
        {
            _db = db;
            _redisCache = distributedCache;
        }

        /// <summary>
        /// Hashes a plaintext password using the SHA-256 cryptographic hash algorithm.
        /// </summary>
        /// <param name="password">The plaintext password to hash.</param>
        /// <returns>A Base64-encoded string representing the SHA-256 hash of the password.</returns>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())  // Creates an instance of the SHA256 hash algorithm
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));  // Hash the password
                return Convert.ToBase64String(hashedBytes);  // Convert the hash to a base64 string for storage
            }
        }

        /// <summary>
        /// Verifies a plaintext password against a stored SHA-256 hashed password.
        /// </summary>
        /// <param name="enteredPassword">The plaintext password entered by the user.</param>
        /// <param name="storedHashedPassword">The hashed password stored in the database.</param>
        /// <returns><c>true</c> if the entered password matches the stored hash; otherwise, <c>false</c>.</returns>
        private bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            var enteredHashedPassword = HashPassword(enteredPassword);
            return enteredHashedPassword == storedHashedPassword;
        }

        /// <summary>
        /// Returns the list of allowed HTTP methods for the users controller.
        /// </summary>
        /// <returns>HTTP 200 OK with the Allow header listing allowed methods.</returns>
        [HttpOptions("options")]
        public IActionResult GetUsersOptions()
        {
            Response.Headers.Append("Allow", "GET, POST, PUT, DELETE, OPTIONS");
            return Ok();
        }

        /// <summary>
        /// Registers a new user and creates an associated cryptocurrency wallet account.
        /// </summary>
        /// <param name="email">The email address of the new user.</param>
        /// <param name="username">The desired username for the new user.</param>
        /// <param name="password">The password for the new user (plaintext).</param>
        /// <returns>
        /// HTTP 200 OK if registration is successful;
        /// HTTP 400 Bad Request if any field is missing;
        /// HTTP 409 Conflict if the email is already registered.
        /// </returns>
        [HttpPut("register")]
        public async Task<IActionResult> Register([FromForm] string email, [FromForm] string username, [FromForm] string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return BadRequest("Email, Username, and Password are required.");

            // Check if the email is already in use
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
                return Conflict("User with this email already exists.");

            // Hash password before storing
            var hashedPassword = HashPassword(password);

            // Save user to the database
            var newUser = new User
            {
                Email = email,
                Name = username,
                Password = hashedPassword,
                ApiKey = Guid.NewGuid()
            };

            // Save user's wallet balance to the database
            var newAccount = new Account
            {
                WalletId = newUser.WalletId,
                Balance = 0,
                Bitcoin = 0,
                Solana = 0,
                Ethereum = 0,
                Ripple = 0,
                Litecoin = 0,
                Cardano = 0,
             };

            await _db.Accounts.AddAsync(newAccount);
            await _db.Users.AddAsync(newUser);
            await _db.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        /// <summary>
        /// Authenticates a user and stores the user session in Redis cache.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password (plaintext).</param>
        /// <returns>
        /// HTTP 200 OK with a login message and API key if credentials are valid;
        /// HTTP 400 Bad Request if inputs are missing;
        /// HTTP 401 Unauthorized if credentials are incorrect.
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return BadRequest("Email and Password are required.");

            // Find the user by email
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return Unauthorized("Invalid email or password.");

            // Check password
            if (!VerifyPassword(password, user.Password))
                return Unauthorized("Invalid email or password.");
            
            var cacheKey = user.ApiKey.ToString();
            var userJson = JsonSerializer.Serialize(user);

            await _redisCache.SetStringAsync(
                key: cacheKey,
                value: userJson,
                options: new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });
                
            return Ok(new { message = "Login successful", apiKey = user.ApiKey });
        }

        /// <summary>
        /// Deletes a user and their associated session and wallet data.
        /// </summary>
        /// <param name="email">The email of the user to delete.</param>
        /// <param name="password">The password for verifying user identity.</param>
        /// <returns>
        /// HTTP 200 OK if the user is deleted successfully;
        /// HTTP 401 Unauthorized if credentials are incorrect;
        /// HTTP 404 Not Found if no user is found.
        /// </returns>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromQuery] string email, [FromForm] string password)
        {
            var user = await _db.Users
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
            
            // Check password
            if (user == null || !VerifyPassword(password, user.Password))
                return Unauthorized("Invalid email or password.");
            
            await _redisCache.RemoveAsync(key: user.ApiKey.ToString());
            await _db.Users
                    .Where(u => u.WalletId == user.WalletId)
                    .ExecuteDeleteAsync();
            await _db.SaveChangesAsync();
            
            return Ok($"User {user.Name} was deleted successfully");
        }
    }
}
