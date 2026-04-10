using Microsoft.EntityFrameworkCore;
using FinanceTracker.Models;

namespace FinanceTracker.Data
{
    public class FTDbContext : DbContext
    {
        
        public FTDbContext(DbContextOptions<FTDbContext> options) : base(options)
        {
        }

        public DbSet<Transactions> Transactions { get; set; }   //DbSet<Transactions> -> Transactions is class name inside model and Another Transactions is Table name
        public DbSet<Users> Users { get; set; }
    }
}

