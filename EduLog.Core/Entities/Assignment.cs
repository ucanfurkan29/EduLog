using System.ComponentModel.DataAnnotations;

namespace EduLog.Core.Entities
{
    public class Assignment
    {
        public int Id { get; set; }

        public int SyllabusWeekId { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // "CodeTask" | "MultipleChoice"

        public bool IsAIGenerated { get; set; }

        public int MaxScore { get; set; }

        public DateTime? DueDate { get; set; }

        public SyllabusWeek Week { get; set; } = null!;
        public ICollection<AssignmentQuestion> Questions { get; set; } = new List<AssignmentQuestion>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
