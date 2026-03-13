using System.ComponentModel.DataAnnotations;

namespace EduLog.Core.Entities
{
    public class WeekResource
    {
        public int Id { get; set; }

        public int SyllabusWeekId { get; set; }

        [Required]
        [MaxLength(300)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ResourceType { get; set; } = string.Empty;

        public SyllabusWeek Week { get; set; } = null!;
    }
}
