using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BugTracker.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        //Database.EnsureCreated();
    }
    public DbSet<Bug> Bugs { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<History> BugHistory { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Models.File> Files { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    public DbSet<Organization> Organizations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Project>()
            .HasIndex(p => p.Key)
            .IsUnique();

        //builder.Entity<Project>()
        //    .HasOne(p => p.Manager)
        //    .WithMany()
        //    .OnDelete(DeleteBehavior.NoAction);

        //builder.Entity<Project>()
        //    .HasOne(p => p.CreatedBy)
        //    .WithMany()
        //    .OnDelete(DeleteBehavior.NoAction);

        //builder.Entity<Bug>()
        //    .HasOne(b => b.AssignedUser)
        //    .WithMany()
        //    .OnDelete(DeleteBehavior.NoAction);

        //builder.Entity<Bug>()
        //    .HasOne(b => b.CreatedBy)
        //    .WithMany()
        //    .OnDelete(DeleteBehavior.NoAction);

        base.OnModelCreating(builder);
    }
}
