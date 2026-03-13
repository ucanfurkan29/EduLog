using System.ComponentModel.DataAnnotations;

namespace EduLog.Core.Entities
{
    public class AssignmentQuestion
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string OptionA { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string OptionB { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string OptionC { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string OptionD { get; set; } = string.Empty;

        [Required]
        [MaxLength(1)]
        public string CorrectAnswer { get; set; } = string.Empty;

        public int OrderIndex { get; set; }

        public Assignment Assignment { get; set; } = null!;
    }
}
