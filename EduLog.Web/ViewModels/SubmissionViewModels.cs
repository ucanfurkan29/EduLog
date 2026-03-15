using System.ComponentModel.DataAnnotations;

namespace EduLog.Web.ViewModels
{
    public class SubmitCodeViewModel
    {
        public int AssignmentId { get; set; }
        public int ClassGroupId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string? AssignmentDescription { get; set; }
        public string? ExpectedBehavior { get; set; }
        public string? StarterCode { get; set; }
        public int MaxScore { get; set; }
        public string ProgrammingLanguage { get; set; } = "csharp";

        [Required(ErrorMessage = "Kod içeriği gereklidir.")]
        [Display(Name = "Kodunuz")]
        public string Content { get; set; } = string.Empty;
    }

    public class SubmitQuizViewModel
    {
        public int AssignmentId { get; set; }
        public int ClassGroupId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public List<QuizQuestionViewModel> Questions { get; set; } = new();
        public List<string> Answers { get; set; } = new();
    }

    public class QuizQuestionViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }

    public class QuizResultViewModel
    {
        public int SubmissionId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
        public int ClassGroupId { get; set; }
        public List<QuizQuestionResultItem> QuestionResults { get; set; } = new();
    }

    public class QuizQuestionResultItem
    {
        public string QuestionText { get; set; } = string.Empty;
        public string StudentAnswer { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
    }

    public class CodeSubmissionResultViewModel
    {
        public int SubmissionId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public int? Score { get; set; }
        public string? InstructorNote { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int ClassGroupId { get; set; }
        public string? SubmittedContent { get; set; }
        public string ProgrammingLanguage { get; set; } = "csharp";

        // AI Review fields
        public string? Summary { get; set; }
        public string? Strengths { get; set; }
        public string? Improvements { get; set; }
        public string? MissingParts { get; set; }
        public bool HasAIReview => !string.IsNullOrWhiteSpace(Summary);
    }

    public class GradeSubmissionViewModel
    {
        public int SubmissionId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int ClassGroupId { get; set; }
        public string AssignmentType { get; set; } = string.Empty;
        public string ProgrammingLanguage { get; set; } = "csharp";
        public string? AssignmentDescription { get; set; }
        public string? ExpectedBehavior { get; set; }

        [Required(ErrorMessage = "Puan gereklidir.")]
        [Range(0, 1000, ErrorMessage = "Geçerli bir puan girin.")]
        [Display(Name = "Puan")]
        public int Score { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Eğitmen Notu")]
        public string? InstructorNote { get; set; }
    }

    public class SubmissionListItemViewModel
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public string AssignmentType { get; set; } = string.Empty;
        public string ContentPreview { get; set; } = string.Empty;
        public int? Score { get; set; }
        public int MaxScore { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
