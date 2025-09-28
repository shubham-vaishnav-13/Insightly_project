using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Insightly_project.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<TaskItemUser> TaskItemUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // important!

            /*  
                For Project --- > Task (one-to-many)
            */
            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");

                // entity.Property(p => p.Name)
                //     .IsRequired()
                //     .HasMaxLength(100);

                // entity.Property(p => p.Description)
                //     .HasMaxLength(500);

                // entity.Property(p => p.CreatedAt)
                //     .HasDefaultValueSql("GETDATE()");

                entity.HasMany(p => p.Tasks)
                    .WithOne(t => t.Project)
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            /* 
                For Project <-> Users (many-to-many)
            */

            modelBuilder.Entity<ProjectUser>(entity =>
            {
                entity.ToTable("ProjectTeamMembers");
                entity.HasKey(pu => new { pu.ProjectId, pu.UserId });

                entity.HasOne(pu => pu.Project)
                    .WithMany(p => p.ProjectUsers)
                    .HasForeignKey(pu => pu.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pu => pu.User)
                    .WithMany(u => u.ProjectUsers)
                    .HasForeignKey(pu => pu.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TaskItem entity configuration  

            /*  
                For TaskItem  
            */


            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.ToTable("TaskItems");

                // entity.Property(t => t.Title)
                //     .IsRequired()
                //     .HasMaxLength(200);

                // entity.Property(t => t.Status)
                //     .IsRequired()
                //     .HasMaxLength(50);

                // entity.Property(t => t.DueDate)
                //     .IsRequired(false);
            });

            // Many-to-many TaskItem <-> Users via TaskItemUser

            /*  
                For TaskItem <-> Users (many-to-many)
            */

            modelBuilder.Entity<TaskItemUser>(entity =>
            {
                entity.ToTable("TaskItemAssignees");
                entity.HasKey(tu => new { tu.TaskItemId, tu.UserId });

                entity.HasOne(tu => tu.TaskItem)
                      .WithMany(t => t.TaskItemUsers)
                      .HasForeignKey(tu => tu.TaskItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tu => tu.User)
                      .WithMany()
                      .HasForeignKey(tu => tu.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
