using Microsoft.EntityFrameworkCore;
using TelcoNet.Data.Entities;

namespace TelcoNet.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<NetworkNode> NetworkNodes => Set<NetworkNode>();
    public DbSet<NetworkMetric> NetworkMetrics => Set<NetworkMetric>();
    public DbSet<Outage> Outages => Set<Outage>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User: unique email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // NetworkNode: unique NodeId
        modelBuilder.Entity<NetworkNode>()
            .HasIndex(n => n.NodeId)
            .IsUnique();

        // NetworkMetric: index for fast queries by region + time
        modelBuilder.Entity<NetworkMetric>()
            .HasIndex(m => new { m.Region, m.Timestamp });

        // ChatSession: relationship
        modelBuilder.Entity<ChatSession>()
            .HasMany(s => s.Messages)
            .WithOne(m => m.ChatSession)
            .HasForeignKey(m => m.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatSession>()
            .HasIndex(s => s.SessionId)
            .IsUnique();

        // Alert: index for severity filtering
        modelBuilder.Entity<Alert>()
            .HasIndex(a => a.Severity);

        // AuditLog: index for timestamp queries
        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.Timestamp);
    }
}
