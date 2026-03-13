using System.ComponentModel.DataAnnotations;

namespace EduLog.Core.Entities
{
    public class ClassGroup
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public int SyllabusId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string JoinCode { get; set; } = string.Empty;

        public int CurrentWeek { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Course Course { get; set; } = null!;
        public Syllabus Syllabus { get; set; } = null!;
        public ICollection<ClassEnrollment> Enrollments { get; set; } = new List<ClassEnrollment>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
