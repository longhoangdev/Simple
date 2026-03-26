using MassTransit;
using Microsoft.EntityFrameworkCore;
using SimpleApp.Domain.Entities.Users;
using SimpleApp.Domain.Interfaces;

namespace SimpleApp.Persistence.Contexts;

public sealed class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);

        // MassTransit Transactional Outbox tables
        builder.AddInboxStateEntity();
        builder.AddOutboxStateEntity();
        builder.AddOutboxMessageEntity();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var softDeleteEntries = ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted).ToList();

        foreach (var entry in softDeleteEntries)
        {
            entry.State = EntityState.Modified;
            entry.Property(nameof(ISoftDeletable.IsDeleted)).CurrentValue = true;
        }
        return SaveChangesAsync(true, cancellationToken);
    }

    public override int SaveChanges() => throw new NotImplementedException();
    public override int SaveChanges(bool acceptAllChangesOnSuccess) => throw new NotImplementedException();
}