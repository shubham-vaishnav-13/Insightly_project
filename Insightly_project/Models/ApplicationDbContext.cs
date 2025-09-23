using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Insightly_project.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // important!

            // Project entity configuration
            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.Description)
                    .HasMaxLength(500);

                entity.Property(p => p.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasMany(p => p.Tasks)
                    .WithOne(t => t.Project)
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TaskItem entity configuration
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.ToTable("TaskItems");

                entity.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(t => t.Status)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(t => t.DueDate)
                    .IsRequired(false);
            });
        }
    }
}
