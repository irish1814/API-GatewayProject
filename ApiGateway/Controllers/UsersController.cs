using ApiGateway.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;


namespace ApiGateway.Controllers
{
    /// <summary>
    /// UsersController provides endpoints for managing user accounts in the Crypto Gateway API.
    /// It supports registration of new users, secure login with password hashing, and account deletion.
    /// Each registered user is assigned a unique API key and an associated crypto wallet initialized with zero balances.
    /// 
    /// Functionalities include:
    /// - <c>PUT /api/Users/register</c>: Register a new user and create their wallet.
    /// - <c>POST /api/Users/login</c>: Authenticate an existing user and retrieve their API key.
    /// - <c>DELETE /api/Users/delete</c>: Delete a user and their wallet after verifying credentials.
    /// - <c>OPTIONS /api/Users/options</c>: Retrieve allowed HTTP methods for introspection.
    /// 
    /// Passwords are hashed using SHA-256 before storage, and user credentials are verified securely during login and deletion.
    /// This controller uses the <see cref="CryptoDbContext"/> to manage persistent user and wallet data.
    /// use this address http://localhost:5182/api/Users to use this API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly CryptoDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class with the specified database context.
        /// </summary>
        /// <param name="db">The database context used to access user data.</param>
        public UsersController(CryptoDbContext db)
        {
            _db = db;
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
        /// <param name="storedHashedPassword">The stored hashed password to compare against.</param>
        /// <returns><c>true</c> if the entered password matches the stored hash; otherwise, <c>false</c>.</returns>
        private bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            var enteredHashedPassword = HashPassword(enteredPassword);
            return enteredHashedPassword == storedHashedPassword;
        }

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
        /// Registers a new user to the Gateway server and create a crypto wallet for him.
        /// </summary>
        /// <param name="username">The username of the new user.</param>
        /// <param name="email">The email address of the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <returns>
        /// 200 OK with the successful registered message if input is valid;
        /// 400 Bad Request if email or password is missing.
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
                Ethereum = 0,
                Ripple = 0,
                Litecoin = 0,
                OtherCrypto = 0,
             };

            await _db.Accounts.AddAsync(newAccount);
            await _db.Users.AddAsync(newUser);
            await _db.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        /// <summary>
        /// Authenticates a user to the Gateway server.
        /// </summary>
        /// <param name="username">The username of the user (optional if email is provided).</param>
        /// <param name="email">The email address of the user (optional if username is provided).</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>
        /// 200 OK with a welcome message and his ApiKey if credentials are valid;
        /// 401 Unauthorized if credentials are invalid.
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
            if (VerifyPassword(password, user.Password))
            {
                return Ok(new { message = "Login successful", apiKey = user.ApiKey });
            } else {
                return Unauthorized("Invalid email or password.");
            }
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
        public async Task<IActionResult> DeleteUser([FromQuery] string email, [FromForm] string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            // Check password
            if (VerifyPassword(password, user.Password))
            {
                var account = await _db.Accounts.FirstOrDefaultAsync(a => a.WalletId == user.WalletId);
                if (account != null)
                    _db.Accounts.Remove(account);
                
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
                return Ok($"User {user.Name} was deleted successfully");
            } else {
                return Unauthorized("Invalid email or password.");
            }
        }

    }
}
