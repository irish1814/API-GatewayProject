using ApiGateway.Models.Entities;
using Microsoft.EntityFrameworkCore;


public class CryptoDbContext : DbContext
{
    public CryptoDbContext(DbContextOptions<CryptoDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users { get; set; }
    
    public DbSet<Account> Accounts { get; set; }

    public DbSet<Transaction> Transactions { get; set; }

    public DbSet<PriceHistory> PriceHistories { get; set; }
}
