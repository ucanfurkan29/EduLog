using System.ComponentModel.DataAnnotations;

namespace EduLog.Core.Entities
{
    public class Syllabus
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Course Course { get; set; } = null!;
        public ICollection<SyllabusWeek> Weeks { get; set; } = new List<SyllabusWeek>();
    }
}
