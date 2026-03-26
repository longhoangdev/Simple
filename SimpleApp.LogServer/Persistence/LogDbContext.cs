using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SimpleApp.LogServer.Domain;
using System.Text.Json;

namespace SimpleApp.LogServer.Persistence;

public sealed class LogDbContext(DbContextOptions<LogDbContext> options) : DbContext(options)
{
    public DbSet<LogEntry> Logs => Set<LogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<LogEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Service).HasMaxLength(100).IsRequired();
            e.Property(x => x.Level).HasMaxLength(20).IsRequired();
            e.Property(x => x.Message).HasMaxLength(4000).IsRequired();
            e.Property(x => x.Source).HasMaxLength(100);
            e.Property(x => x.Hostname).HasMaxLength(100);
            e.Property(x => x.Tags).HasMaxLength(500);
            e.Property(x => x.Attributes)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null))
                .HasColumnType("jsonb")
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, object>?>(
                    (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
                    c => c == null ? 0 : JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
                    c => c == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(c, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)));

            e.HasIndex(x => x.Timestamp);
            e.HasIndex(x => x.Level);
            e.HasIndex(x => x.Service);
        });
    }
}
