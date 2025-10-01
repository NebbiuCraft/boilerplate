using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Base configuration for all entities that inherit from Entity
/// Configures common properties like Id and Active
/// </summary>
public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : Entity
{
  public virtual void Configure(EntityTypeBuilder<T> builder)
  {
    // Configure primary key
    builder.HasKey(e => e.Id);

    // Configure Id property
    builder.Property(e => e.Id)
        .ValueGeneratedOnAdd()
        .IsRequired();

    // Configure Active property with default value
    builder.Property(e => e.Active)
        .IsRequired()
        .HasDefaultValue(true);

    // Create index on Active for performance
    builder.HasIndex(e => e.Active)
        .HasDatabaseName($"IX_{typeof(T).Name}_Active");

    // Call derived configuration
    ConfigureEntity(builder);
  }

  /// <summary>
  /// Override this method in derived configurations to add entity-specific configuration
  /// </summary>
  /// <param name="builder">The entity type builder</param>
  protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
}