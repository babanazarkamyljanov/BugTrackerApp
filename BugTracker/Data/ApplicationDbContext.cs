using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BugTracker.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        //Database.EnsureCreated();
    }
    public DbSet<Bug> Bugs { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<BugHistory> BugHistory { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<BugFile> Files { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    public DbSet<Organization> Organizations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Project>()
            .HasIndex(p => p.ProjectKey)
            .IsUnique();

        base.OnModelCreating(builder);
    }
}
