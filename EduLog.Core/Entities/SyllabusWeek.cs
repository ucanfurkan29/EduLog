using System.ComponentModel.DataAnnotations;

namespace EduLog.Core.Entities
{
    public class SyllabusWeek
    {
        public int Id { get; set; }

        public int SyllabusId { get; set; }

        public int WeekNumber { get; set; }

        [Required]
        [MaxLength(300)]
        public string Topic { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Examples { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public Syllabus Syllabus { get; set; } = null!;
        public ICollection<WeekResource> Resources { get; set; } = new List<WeekResource>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
