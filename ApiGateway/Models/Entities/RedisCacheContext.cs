using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;


namespace  ApiGateway.Models.Entities
{
    /// <summary>
    /// Provides methods for interacting with Redis cache for storing and retrieving User and Account data.
    /// </summary>
    public class RedisCacheContext
    {
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheContext"/> class with the specified cache provider.
        /// </summary>
        /// <param name="cache">An instance of <see cref="IDistributedCache"/>.</param>
        public RedisCacheContext(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Retrieves a list of transaction history entries for a specific user from Redis cache.
        /// </summary>
        /// <param name="apiKey">The API key used to uniquely identify the user.</param>
        /// <param name="walletId">The wallet ID associated with the user's account.</param>
        /// <returns>
        /// A list of <see cref="Transaction"/> objects if found in Redis; otherwise, <c>null</c>.
        /// </returns>
        public async Task<List<Transaction>?> GetTransactionHistory(string apiKey, string walletId)
        {
            var cacheKey = $"{apiKey}:{walletId}";  // Create a unique cache key

            Console.WriteLine($"Fetching list of transaction history from Redis.");

            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedData))
                return null;

            var transactionList = JsonSerializer.Deserialize<List<Transaction>>(cachedData);
            return transactionList;
        }
        
        /// <summary>
        /// Stores a list of transaction history entries for a specific user in Redis cache.
        /// </summary>
        /// <param name="apiKey">The API key used to uniquely identify the user.</param>
        /// <param name="walletId">The wallet ID associated with the user's account.</param>
        /// <param name="transactionList">A list of <see cref="Transaction"/> objects to cache.</param>
        /// <param name="expiration">
        /// Optional. The time span after which the cache entry should expire. Defaults to 1 hour if not specified.
        /// </param>
        /// <returns>A task representing the asynchronous cache operation.</returns>
        public async Task SetTransactionHistory(string apiKey, string walletId, List<Transaction> transactionList, TimeSpan? expiration = null)
        {
            var cacheKey = $"{apiKey}:{walletId}";  // Unique cache key
            var json = JsonSerializer.Serialize(transactionList);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
            };

            Console.WriteLine($"Storing list of transaction history in Redis.");
            await _cache.SetStringAsync(cacheKey, json, options);
        }

        /// <summary>
        /// Retrieves a cached <see cref="User"/> by their API key from Redis.
        /// </summary>
        /// <param name="apiKey">The API key of the user.</param>
        /// <returns>A <see cref="User"/> object if found; otherwise, null.</returns>
        public async Task<User?> GetUserByApiKey(string apiKey)
        {
            var cachedData = await _cache.GetStringAsync(apiKey);

            if (string.IsNullOrEmpty(cachedData))
                return null;
            
            // Parse or return cached data
            Console.WriteLine("Fetching user from Redis");
            var userFromCache = JsonSerializer.Deserialize<User>(cachedData);
            return userFromCache;
        }

        /// <summary>
        /// Retrieves a cached <see cref="Account"/> by wallet ID from Redis.
        /// </summary>
        /// <param name="walletId">The wallet ID of the account.</param>
        /// <returns>An <see cref="Account"/> object if found; otherwise, null.</returns>
        public async Task<Account?> GetAccountByWalletId(string walletId)
        {
            var cachedData = await _cache.GetStringAsync(walletId);

            if (string.IsNullOrEmpty(cachedData))
                return null;
            
            // Parse or return cached data
            Console.WriteLine("Fetching account from Redis");
            var accountFromCache = JsonSerializer.Deserialize<Account>(cachedData);
            return accountFromCache;
        }
        
        /// <summary>
        /// Stores a <see cref="User"/> object in Redis cache with the provided API key as the key.
        /// </summary>
        /// <param name="apiKey">The API key used as the Redis key.</param>
        /// <param name="user">The <see cref="User"/> object to cache.</param>
        /// <param name="expiration">Optional expiration time for the cached item. Defaults to 1 hour.</param>
        public async Task SetUserByApiKey(string apiKey, User user, TimeSpan? expiration = null)
        {
            Console.WriteLine("Storing new user into Redis");
            var json = JsonSerializer.Serialize(user);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
            };

            await _cache.SetStringAsync(apiKey, json, options);
        }

        /// <summary>
        /// Stores an <see cref="Account"/> object in Redis cache using the wallet ID as the key.
        /// </summary>
        /// <param name="walletId">The wallet ID used as the Redis key.</param>
        /// <param name="account">The <see cref="Account"/> object to cache.</param>
        /// <param name="expiration">Optional expiration time for the cached item. Defaults to 1 hour.</param>
        public async Task SetAccountByWalletId(string walletId, Account account, TimeSpan? expiration = null)
        {
            Console.WriteLine("Storing new account into Redis");
            var json = JsonSerializer.Serialize(account);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
            };

            await _cache.SetStringAsync(walletId, json, options);
        }
        
        /// <summary>
        /// Removes a key and its associated value from Redis cache.
        /// </summary>
        /// <param name="key">The Redis key to remove.</param>
        public async Task Remove(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}