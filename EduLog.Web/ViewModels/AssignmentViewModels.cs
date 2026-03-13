using System.ComponentModel.DataAnnotations;

namespace EduLog.Web.ViewModels
{
    public class CreateAssignmentViewModel
    {
        public int SyllabusWeekId { get; set; }
        public string WeekTopic { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int WeekNumber { get; set; }
        public int SyllabusId { get; set; }

        [Required(ErrorMessage = "Başlık gereklidir.")]
        [MaxLength(300)]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Tür seçimi gereklidir.")]
        [Display(Name = "Ödev Türü")]
        public string Type { get; set; } = "CodeTask";

        [Required(ErrorMessage = "Maksimum puan gereklidir.")]
        [Range(1, 1000, ErrorMessage = "Puan 1-1000 arasında olmalıdır.")]
        [Display(Name = "Maksimum Puan")]
        public int MaxScore { get; set; } = 100;

        [Display(Name = "Son Teslim Tarihi")]
        public DateTime? DueDate { get; set; }

        public List<QuestionViewModel> Questions { get; set; } = new();
    }

    public class EditAssignmentViewModel
    {
        public int Id { get; set; }
        public int SyllabusWeekId { get; set; }
        public string WeekTopic { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int WeekNumber { get; set; }
        public int SyllabusId { get; set; }

        [Required(ErrorMessage = "Başlık gereklidir.")]
        [MaxLength(300)]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Tür seçimi gereklidir.")]
        [Display(Name = "Ödev Türü")]
        public string Type { get; set; } = "CodeTask";

        [Required(ErrorMessage = "Maksimum puan gereklidir.")]
        [Range(1, 1000, ErrorMessage = "Puan 1-1000 arasında olmalıdır.")]
        [Display(Name = "Maksimum Puan")]
        public int MaxScore { get; set; } = 100;

        [Display(Name = "Son Teslim Tarihi")]
        public DateTime? DueDate { get; set; }

        public List<QuestionViewModel> Questions { get; set; } = new();
    }

    public class QuestionViewModel
    {
        [Required(ErrorMessage = "Soru metni gereklidir.")]
        [MaxLength(2000)]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "A şıkkı gereklidir.")]
        [MaxLength(500)]
        public string OptionA { get; set; } = string.Empty;

        [Required(ErrorMessage = "B şıkkı gereklidir.")]
        [MaxLength(500)]
        public string OptionB { get; set; } = string.Empty;

        [Required(ErrorMessage = "C şıkkı gereklidir.")]
        [MaxLength(500)]
        public string OptionC { get; set; } = string.Empty;

        [Required(ErrorMessage = "D şıkkı gereklidir.")]
        [MaxLength(500)]
        public string OptionD { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğru cevap seçimi gereklidir.")]
        [MaxLength(1)]
        public string CorrectAnswer { get; set; } = "A";

        public int OrderIndex { get; set; }
    }

    public class AIPreviewViewModel
    {
        public int SyllabusWeekId { get; set; }
        public string WeekTopic { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int WeekNumber { get; set; }
        public int SyllabusId { get; set; }
        public string Provider { get; set; } = "Claude (Anthropic)";

        [Required(ErrorMessage = "Başlık gereklidir.")]
        [MaxLength(300)]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Maksimum puan gereklidir.")]
        [Range(1, 1000)]
        [Display(Name = "Maksimum Puan")]
        public int MaxScore { get; set; } = 100;

        [Display(Name = "Son Teslim Tarihi")]
        public DateTime? DueDate { get; set; }

        public List<QuestionViewModel> Questions { get; set; } = new();
    }

    public class AICodeTaskPreviewViewModel
    {
        public int SyllabusWeekId { get; set; }
        public string WeekTopic { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int WeekNumber { get; set; }
        public int SyllabusId { get; set; }
        public string Provider { get; set; } = "Claude (Anthropic)";

        [Required(ErrorMessage = "Başlık gereklidir.")]
        [MaxLength(300)]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "Açıklama (Ödev Yönergesi)")]
        public string? Description { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Beklenen Davranış / Çıktı")]
        public string? ExpectedBehavior { get; set; }

        [Display(Name = "Başlangıç Kodu")]
        public string? StarterCode { get; set; }

        [Required(ErrorMessage = "Maksimum puan gereklidir.")]
        [Range(1, 1000)]
        [Display(Name = "Maksimum Puan")]
        public int MaxScore { get; set; } = 100;

        [Display(Name = "Son Teslim Tarihi")]
        public DateTime? DueDate { get; set; }
    }
}
