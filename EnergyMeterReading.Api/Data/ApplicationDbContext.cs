using EnergyMeterReading.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EnergyMeterReading.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<MeterReading> MeterReadings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Account>()
            .Property(a => a.Id)
            .ValueGeneratedNever();

            modelBuilder.Entity<MeterReading>()
            .HasOne(mr => mr.Account)
            .WithMany(a => a.MeterReadings)
            .HasForeignKey(mr => mr.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeterReading>()
               .HasIndex(m => new { m.AccountId, m.ReadingDate })
               .IsUnique();
        }
    }
}