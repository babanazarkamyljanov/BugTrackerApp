using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BugTracker.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        //Database.EnsureCreated();
    }
    public DbSet<Bug> Bugs => Set<Bug>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<BugHistory> BugHistory => Set<BugHistory>();

    public DbSet<BugComment> BugComments => Set<BugComment>();

    public DbSet<Models.BugFile> BugFiles => Set<Models.BugFile>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Organization> Organizations => Set<Organization>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Project>()
            .HasOne(p => p.Manager)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Project>()
            .HasOne(p => p.CreatedBy)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Bug>()
            .HasOne(b => b.Project)
            .WithMany(p => p.Bugs)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Bug>()
            .HasOne(b => b.CreatedBy)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Bug>()
            .HasOne(b => b.Assignee)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<BugHistory>()
            .HasOne(bh => bh.Bug)
            .WithMany(b => b.History)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<BugComment>()
            .HasOne(c => c.Bug)
            .WithMany(b => b.Comments)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<BugFile>()
            .HasOne(f => f.Bug)
            .WithMany(b => b.Files)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Organization>()
            .HasIndex(o => o.Name)
            .IsUnique();

        base.OnModelCreating(builder);
    }
}
