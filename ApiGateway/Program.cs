using ApiGateway.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<CryptoDbContext>();
builder.Services.AddScoped<RedisCacheContext>();


string? connectionString = builder.Configuration.GetConnectionString("APIServer");
builder.Services.AddDbContextPool<CryptoDbContext>(options => options
    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

builder.Services.AddStackExchangeRedisCache(redisOptions =>
{
    redisOptions.Configuration = builder.Configuration.GetConnectionString("Redis");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Connection checks before app.Run()
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    // Check MySQL connection
    var db = serviceProvider.GetRequiredService<CryptoDbContext>();
    try
    {
        if (!db.Database.CanConnect())
            throw new Exception("❌ Failed to connect to MySQL database.");
        Console.WriteLine("✅ MySQL connection successful.");
    }
    catch
    {
        Console.WriteLine("❌ Failed to connect to MySQL database.");
        throw; // Stop the app
    }

    // Check Redis connection
    var cache = serviceProvider.GetRequiredService<IDistributedCache>();
    try
    {
        const string testKey = "redis_test_key";
        await cache.SetStringAsync(testKey, "ok", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
        });

        var value = await cache.GetStringAsync(testKey);
        if (value != "ok")
            throw new Exception("❌ Failed to connect to Redis database.");

        Console.WriteLine("✅ Redis connection successful.");
    }
    catch
    {
        Console.WriteLine("❌ Failed to connect to Redis database.");
        throw; // Stop the app
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=index}/{id?}");


app.Run();
