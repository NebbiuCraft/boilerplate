using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// DbContext for the legacy database system
/// Generated using EF Core Database-First approach
/// </summary>
public partial class LegacyDbContext : DbContext
{
  public LegacyDbContext(DbContextOptions<LegacyDbContext> options)
      : base(options)
  {
  }

  // DbSets will be added here when you scaffold from the database
  // Example:
  // public virtual DbSet<LegacyTable> LegacyTables { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // Model configuration will be added here when you scaffold from the database
    OnModelCreatingPartial(modelBuilder);
  }

  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}