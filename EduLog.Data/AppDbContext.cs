using EduLog.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Syllabus> Syllabi { get; set; }
        public DbSet<SyllabusWeek> SyllabusWeeks { get; set; }
        public DbSet<WeekResource> WeekResources { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentQuestion> AssignmentQuestions { get; set; }
        public DbSet<ClassGroup> ClassGroups { get; set; }
        public DbSet<ClassEnrollment> ClassEnrollments { get; set; }
        public DbSet<Submission> Submissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // JoinCode unique constraint
            builder.Entity<ClassGroup>()
                .HasIndex(cg => cg.JoinCode)
                .IsUnique();

            // Course relationships
            builder.Entity<Course>()
                .HasMany(c => c.Syllabi)
                .WithOne(s => s.Course)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Course>()
                .HasMany(c => c.ClassGroups)
                .WithOne(cg => cg.Course)
                .HasForeignKey(cg => cg.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Syllabus relationships
            builder.Entity<Syllabus>()
                .HasMany(s => s.Weeks)
                .WithOne(w => w.Syllabus)
                .HasForeignKey(w => w.SyllabusId)
                .OnDelete(DeleteBehavior.Cascade);

            // SyllabusWeek relationships
            builder.Entity<SyllabusWeek>()
                .HasMany(w => w.Resources)
                .WithOne(r => r.Week)
                .HasForeignKey(r => r.SyllabusWeekId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SyllabusWeek>()
                .HasMany(w => w.Assignments)
                .WithOne(a => a.Week)
                .HasForeignKey(a => a.SyllabusWeekId)
                .OnDelete(DeleteBehavior.Cascade);

            // Assignment relationships
            builder.Entity<Assignment>()
                .HasMany(a => a.Questions)
                .WithOne(q => q.Assignment)
                .HasForeignKey(q => q.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Assignment>()
                .HasMany(a => a.Submissions)
                .WithOne(s => s.Assignment)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ClassGroup relationships
            builder.Entity<ClassGroup>()
                .HasOne(cg => cg.Syllabus)
                .WithMany()
                .HasForeignKey(cg => cg.SyllabusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClassGroup>()
                .HasMany(cg => cg.Enrollments)
                .WithOne(e => e.ClassGroup)
                .HasForeignKey(e => e.ClassGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClassGroup>()
                .HasMany(cg => cg.Submissions)
                .WithOne(s => s.ClassGroup)
                .HasForeignKey(s => s.ClassGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // ClassEnrollment relationships
            builder.Entity<ClassEnrollment>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Submission relationships
            builder.Entity<Submission>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed courses
            builder.Entity<Course>().HasData(
                new Course { Id = 1, Name = "Python", Description = "Python programlama dili eğitimi" },
                new Course { Id = 2, Name = "C#", Description = "C# programlama dili eğitimi" },
                new Course { Id = 3, Name = "SQL Server", Description = "SQL Server veritabanı yönetimi eğitimi" },
                new Course { Id = 4, Name = "Unity", Description = "Unity oyun motoru eğitimi" },
                new Course { Id = 5, Name = "Web Yazılım", Description = "Web geliştirme eğitimi" }
            );
        }
    }
}
