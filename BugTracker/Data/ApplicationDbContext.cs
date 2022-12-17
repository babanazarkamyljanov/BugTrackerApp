using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BugTracker.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
    public DbSet<Bug> Bugs { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<AppUserBug> AppUserBug { get; set; }
    public DbSet<AppUserProject> AppUserProject { get; set; }
    public DbSet<BugHistory> BugHistories { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<FileOfBug> Files { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AppUserBug>()
            .HasKey(ab => new { ab.AppUserId, ab.BugId });
        builder.Entity<AppUserBug>()
            .HasOne(ab => ab.AppUser)
            .WithMany(a => a.AppUsersForBugs)
            .HasForeignKey(ab => ab.AppUserId);
        builder.Entity<AppUserBug>()
            .HasOne(ab => ab.Bug)
            .WithMany(b => b.AssignedUsersForBug)
            .HasForeignKey(ab => ab.BugId);

        builder.Entity<AppUserProject>()
            .HasKey(ap => new { ap.AppUserId, ap.ProjectId });
        builder.Entity<AppUserProject>()
            .HasOne(ap => ap.AppUser)
            .WithMany(a => a.AppUsersForProjects)
            .HasForeignKey(ap => ap.AppUserId);
        builder.Entity<AppUserProject>()
            .HasOne(ab => ab.Project)
            .WithMany(b => b.AssignedUsersForProject)
            .HasForeignKey(ab => ab.ProjectId);

        base.OnModelCreating(builder);
    }
}
