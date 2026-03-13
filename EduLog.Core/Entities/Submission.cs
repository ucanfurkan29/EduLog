using System.ComponentModel.DataAnnotations;

namespace EduLog.Core.Entities
{
    public class Submission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        public int ClassGroupId { get; set; }

        public int UserId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? Score { get; set; }

        [MaxLength(2000)]
        public string? InstructorNote { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public Assignment Assignment { get; set; } = null!;
        public ClassGroup ClassGroup { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
