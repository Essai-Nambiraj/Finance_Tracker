using Microsoft.EntityFrameworkCore;
using FinanceTracker.Models;

namespace FinanceTracker.Data
{
    public class FTDbContext : DbContext
    {
        
        public FTDbContext(DbContextOptions<FTDbContext> options) : base(options)
        {
        }

        public DbSet<Transactions> Transactions { get; set; }
        public DbSet<Users> Users { get; set; }
    }
}

