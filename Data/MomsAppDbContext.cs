using Microsoft.EntityFrameworkCore;
using MomsAppApi.Entities;
using MomsAppApi.Models;

namespace MomsAppApi.Data
{
    public class MomsAppDbContext(DbContextOptions<MomsAppDbContext> options) : DbContext(options)
    {
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAccount>()
                .HasKey(u => u.user_id);

            modelBuilder.Entity<UserAccount>()
                .HasIndex(u => u.email)
                .IsUnique();

            modelBuilder.Entity<UserAccount>()
                .HasIndex(u => u.employee_id)
                .IsUnique(); // enforces 1-to-1

            modelBuilder.Entity<Employee>()
               .HasKey(e => e.employee_id);


            base.OnModelCreating(modelBuilder);
        }


    }
}