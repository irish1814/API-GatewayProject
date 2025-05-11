using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ApiGateway.Models.Entities;
using Microsoft.EntityFrameworkCore;


namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controller responsible for handling Transaction service calls including Buy, Sell,
    /// checking wallet balance and add cash into user's balance
    /// Access this controller using http://hostip:5182/Transactions/{Operation}
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : Controller
    {
        private readonly CryptoDbContext _db;
        private readonly RedisCacheContext _redisCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class with the specified database context.
        /// </summary>
        /// <param name="db">The database context used to access user data.</param>
        /// <param name="distributedCache">Redis database context used to access logged-in user data.</param>
        public TransactionsController(CryptoDbContext db, RedisCacheContext distributedCache)
        {
            _db = db;
            _redisCache = distributedCache;
        }

        /// <summary>
        /// Updates the cryptocurrency balance in the user's account based on symbol and amount.
        /// </summary>
        /// <param name="account">The user's account object to be updated.</param>
        /// <param name="symbol">The symbol of the cryptocurrency (e.g., BTC, ETH).</param>
        /// <param name="amount">The amount to adjust (positive for buy, negative to sell).</param>
        private static void UpdateCryptoBalance(Account account, string symbol, decimal amount)
        {
            switch (symbol)
            {
                case "BTC": account.Bitcoin += amount; break;
                case "ETH": account.Ethereum += amount; break;
                case "LTC": account.Litecoin += amount; break;
                case "XRP": account.Ripple += amount; break;
                default: account.OtherCrypto += amount; break;
            }
        }

        /// <summary>
        /// Checks whether the user has sufficient balance of a specific cryptocurrency.
        /// </summary>
        /// <param name="account">The user's account object.</param>
        /// <param name="symbol">The symbol of the cryptocurrency.</param>
        /// <param name="required">The amount required for the transaction.</param>
        /// <returns>
        /// true if the user has enough balance; otherwise false.
        /// </returns>
        private static bool HasSufficientCrypto(Account account, string symbol, decimal required)
        {
            return symbol switch
            {
                "BTC" => account.Bitcoin >= required,
                "ETH" => account.Ethereum >= required,
                "LTC" => account.Litecoin >= required,
                "XRP" => account.Ripple >= required,
                _ => account.OtherCrypto >= required
            };
        }
        
                /// <summary>
        /// Retrieves a user object by their API key.
        /// Checks Redis cache first, then falls back to MySQL if not found.
        /// </summary>
        /// <param name="apiKey">The API key associated with the user.</param>
        /// <returns>
        /// A <see cref="User"/> object if found; otherwise null.
        /// </returns>
        private async Task<User?> GetUserByApiKey(string apiKey)
        {
            var cachedUser = await _redisCache.GetUserByApiKey(apiKey);

            if (cachedUser != null)
            {
                // Parse or return cached data
                return cachedUser;
            }
            
            Console.WriteLine("Fetching user from MySQL");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.ApiKey == Guid.Parse(apiKey));
            
            if(user == null)
                return null;
            
            await _redisCache.SetUserByApiKey(apiKey, user);
            return user;
        }

        /// <summary>
        /// Retrieves an account associated with the given API key (via user's wallet ID).
        /// Checks Redis cache first, then falls back to MySQL if not found.
        /// </summary>
        /// <param name="apiKey">The user's API key.</param>
        /// <returns>
        /// An <see cref="Account"/> object if found; otherwise null.
        /// </returns>
        private async Task<Account?> GetAccountByApiKey(string apiKey)
        {
            var user = await GetUserByApiKey(apiKey);
            if (user == null)
                return null;
            
            var cachedAccount = await _redisCache
                .GetAccountByWalletId(user.WalletId.ToString());

            if (cachedAccount != null)
            {
                // Parse or return cached data
                return cachedAccount;
            }
            
            Console.WriteLine("Fetching account from MySQL");
            var account = await _db.Accounts
                .FirstOrDefaultAsync(a => a.WalletId == Guid.Parse(user.WalletId.ToString()));
            
            if (account == null)
                return null;
            
            await _redisCache.SetAccountByWalletId(user.WalletId.ToString(), account);
            return account;
        }
        
        /// <summary>
        /// Handles cryptocurrency buy/sell transactions for a user.
        /// Validates API key, fetches current currency price, and updates balances accordingly.
        /// </summary>
        /// <param name="id">The Coinlore currency ID.</param>
        /// <param name="apiKey">The user's API key.</param>
        /// <param name="type">Transaction type: "buy" or "sell".</param>
        /// <param name="amount">The amount of cryptocurrency to transact.</param>
        /// <returns>
        /// 200 OK if the transaction was successful;
        /// 400 BadRequest if there are insufficient funds or crypto;
        /// 401 Unauthorized if the API key is invalid;
        /// 404 NotFound if currency or account not found;
        /// 500 InternalServerError if the external API call fails.
        /// </returns>
        private async Task<IActionResult> HandleTransaction(int id, string apiKey, string type, decimal amount)
        {
            var user = await GetUserByApiKey(apiKey);
            if (user == null)
                return Unauthorized("Invalid or missing API key: X-Api-Key=YOUR-API-KEY");

            // Remove account and transactions list of the user from the cache to stay up to date
            await _redisCache.Remove(user.WalletId.ToString());
            await _redisCache.Remove($"{apiKey}:{user.WalletId.ToString()}");
            
            var account = _db.Accounts.FirstOrDefault(a => a.WalletId == user.WalletId);
            
            if (account == null)
                return NotFound("Account not found.");

            var response = await new HttpClient().GetAsync($"https://api.coinlore.net/api/ticker/?id={id}");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Failed to fetch currency info.");

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(content);
            if (data == null || data.Count == 0)
                return NotFound("Currency not found.");

            var coin = data[0];
            var symbol = coin["symbol"].ToString()!.ToUpper();
            var priceUsd = decimal.Parse(coin["price_usd"].ToString() ?? "0");

            var totalCost = priceUsd * amount;

            switch (type)
            {
                case "buy":
                    if (account.Balance < totalCost)
                        return BadRequest("Insufficient funds.");
                    account.Balance -= totalCost;
                    UpdateCryptoBalance(account, symbol, amount);
                    break;

                case "sell":
                    if (!HasSufficientCrypto(account, symbol, amount))
                        return BadRequest("Insufficient crypto balance.");
                    UpdateCryptoBalance(account, symbol, -amount);
                    account.Balance += totalCost;
                    break;

                case "default":
                    return BadRequest("Invalid transaction type.");
            }

            _db.Transactions.Add(new Transaction
            {
                WalletId = user.WalletId,
                CryptoId = id,
                PriceAtTransaction = priceUsd,
                Amount = amount,
                Type = type,
                DateTime = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            
            return Ok($"Successfully {type}ed {amount} {symbol} at ${priceUsd} each.");
        }
                
        /// <summary>
        /// Adds the specified amount of money to the authenticated user's account balance.
        /// </summary>
        /// <param name="amount">The amount of money to add. Passed as a form field.</param>
        /// <param name="apiKey">The API key identifying the user. Must be passed in the request header as 'X-Api-Key'.</param>
        /// <returns>
        /// Returns an HTTP 200 OK response with a success message and updated balance on success.
        /// Returns HTTP 401 Unauthorized if the API key is invalid.
        /// Returns HTTP 404 Not Found if the user's account cannot be found.
        /// </returns>
        [HttpPost("AddMoney")]
        public async Task<IActionResult> AddMoney([FromForm] decimal amount, [FromHeader(Name = "X-Api-Key")] string apiKey)
        {
            var user = await GetUserByApiKey(apiKey);
            if (user == null)
                return Unauthorized("Invalid or missing API key: X-Api-Key=YOUR-API-KEY");

            await _redisCache.Remove(user.WalletId.ToString());
            var account = _db.Accounts.FirstOrDefault(a => a.WalletId == user.WalletId);
            
            if (account == null)
                return NotFound("Account not found.");
            
            account.Balance += amount;
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Successfully added {amount:C} to your balance.", newBalance = account.Balance });
        }

        /// <summary>
        /// Initiates a Buy crypto transaction
        /// </summary>
        /// <param name="id">The Coinlore ID of the cryptocurrency</param>
        /// <param name="apiKey">The API Key of the user initiating the transaction</param>
        /// <param name="amount">The amount of currencies to buy</param>
        /// <returns>A response indicating the success or failure of the transaction</returns>
        [HttpPost("Buy")]
        public async Task<IActionResult> BuyCrypto([FromForm] int id, [FromHeader(Name = "X-Api-Key")] string apiKey, [FromForm] decimal amount)
        {
            return await HandleTransaction(id, apiKey, "buy", amount);
        }

        /// <summary>
        /// Initiates a Sell crypto transaction
        /// </summary>
        /// <param name="id">The Coinlore ID of the cryptocurrency</param>
        /// <param name="apiKey">The API Key of the user initiating the transaction</param>
        /// <param name="amount">The amount of currencies to sell</param>
        /// <returns>A response indicating the success or failure of the transaction</returns>
        [HttpPost("Sell")]
        public async Task<IActionResult> SellCrypto([FromForm] int id, [FromHeader(Name = "X-Api-Key")] string apiKey, [FromForm] decimal amount)
        {
            return await HandleTransaction(id, apiKey, "sell", amount);
        }
        
        /// <summary>
        /// Retrieves the current wallet balance for the user associated with the provided API key.
        /// </summary>
        /// <param name="apiKey">The API key identifying the user. Must be passed in the request header as 'X-Api-Key'.</param>
        /// <returns>
        /// Returns an HTTP 200 OK response containing the wallet balance if the user is authenticated.
        /// Returns HTTP 401 Unauthorized if the API key is invalid or missing.
        /// Returns HTTP 404 Not Found if the user's account cannot be found.
        /// </returns>
        [HttpGet("WalletBalance")]
        public async Task<IActionResult> GetWalletBalance([FromHeader(Name = "X-Api-Key")] string apiKey) 
        {
            var account = await GetAccountByApiKey(apiKey);
            if (account == null)
                return NotFound("Account not found.");
            
            return Ok(new { WalletBalance = account });
        }
        
        [HttpGet("TransactionsHistory")]
        public async Task<IActionResult> GetTransactionHistory([FromHeader(Name = "X-Api-Key")] string apiKey)
        {
            var account = await GetAccountByApiKey(apiKey);
            if (account == null)
                return NotFound("Account not found.");
            
            var cachedHistory = await _redisCache.GetTransactionHistory(apiKey, account.WalletId.ToString());
            if (cachedHistory != null)
                return Ok(new { TransactionsHistory = cachedHistory });
            
            Console.WriteLine("Fetching transaction history from MySQL");
            var userTransactionHistory = await _db.Transactions
                .Where(t => t.WalletId == account.WalletId)
                .OrderBy(t => t.DateTime)
                .ToListAsync();
            
            await _redisCache.SetTransactionHistory(apiKey, account.WalletId.ToString(), userTransactionHistory); 
            return Ok(new { TransactionsHistory = userTransactionHistory });
        }
    }
}