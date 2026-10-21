using csharp_math.Models;
using Microsoft.EntityFrameworkCore;

namespace csharp_math.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<FunctionAnalysis> FunctionAnalyses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FunctionAnalysis>(entity =>
            {
                entity.ToTable("FunctionAnalyses");
                entity.HasIndex(e => e.CreatedAt);

                entity.Property(e => e.UpdatedAt).HasConversion(
                    v => v.HasValue ? v.Value.ToUniversalTime() : v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

                // SQLite has no time zone information; values are stored and read back as UTC.
                entity.Property(e => e.CreatedAt).HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            });
        }
    }
}
