namespace EduLog.Web.ViewModels
{
    public class StudentClassViewModel
    {
        public int ClassGroupId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int CurrentWeek { get; set; }
    }

    public class StudentClassDetailViewModel
    {
        public int ClassGroupId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int CurrentWeek { get; set; }
        public int TotalScore { get; set; }
        public int ClassRank { get; set; }
        public List<StudentWeekItemViewModel> OpenWeeks { get; set; } = new();
    }

    public class StudentWeekItemViewModel
    {
        public int WeekNumber { get; set; }
        public string Topic { get; set; } = string.Empty;
        public int WeekScore { get; set; }
    }

    public class StudentWeekDetailViewModel
    {
        public int ClassGroupId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int WeekNumber { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? Examples { get; set; }
        public List<StudentResourceViewModel> Resources { get; set; } = new();
        public List<StudentAssignmentViewModel> Assignments { get; set; } = new();
    }

    public class StudentResourceViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
    }

    public class StudentAssignmentViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public DateTime? DueDate { get; set; }
        public bool HasSubmission { get; set; }
        public int? SubmissionId { get; set; }
        public int? Score { get; set; }
        public string? ExpectedBehavior { get; set; }
        public string? StarterCode { get; set; }
    }
}
