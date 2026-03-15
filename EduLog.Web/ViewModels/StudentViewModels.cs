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

    public class StudentProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public int TotalScore { get; set; }
        public int CompletedAssignmentCount { get; set; }
        public int TotalAssignmentCount { get; set; }
        public int EnrolledClassCount { get; set; }
        public double OverallSuccessRate { get; set; }
        public List<CourseStatViewModel> CourseStats { get; set; } = new();
        public List<WeeklyScoreViewModel> WeeklyScores { get; set; } = new();
    }

    public class CourseStatViewModel
    {
        public string CourseName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int ClassGroupId { get; set; }
        public int TotalScore { get; set; }
        public int MaxPossibleScore { get; set; }
        public int CompletedCount { get; set; }
        public int TotalCount { get; set; }
        public double SuccessRate { get; set; }
        public int Rank { get; set; }
    }

    public class WeeklyScoreViewModel
    {
        public string Label { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    // ── Shared: Class Selector ────────────────────────────
    public class ClassSelectorItem
    {
        public int ClassGroupId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string DisplayName => $"{CourseName} — {ClassName}";
    }

    // ── MyAssignments (Ödevlerim) ────────────────────────────
    public class MyAssignmentsViewModel
    {
        public int TotalAssignmentCount { get; set; }
        public int SubmittedCount { get; set; }
        public int PendingCount { get; set; }
        public double OverallSuccessRate { get; set; }
        public int? SelectedClassGroupId { get; set; }
        public List<ClassSelectorItem> AvailableClasses { get; set; } = new();
        public List<MyAssignmentClassGroup> ClassGroups { get; set; } = new();
    }

    public class MyAssignmentClassGroup
    {
        public int ClassGroupId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public List<MyAssignmentItemViewModel> Assignments { get; set; } = new();
    }

    public class MyAssignmentItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public int WeekNumber { get; set; }
        public string WeekTopic { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public bool HasSubmission { get; set; }
        public int? SubmissionId { get; set; }
        public int? Score { get; set; }
        public int ClassGroupId { get; set; }
    }

    // ── Leaderboard (Sıralama) ────────────────────────────
    public class LeaderboardViewModel
    {
        public int? SelectedClassGroupId { get; set; }
        public string SelectedClassName { get; set; } = string.Empty;
        public string SelectedCourseName { get; set; } = string.Empty;
        public int CurrentUserId { get; set; }
        public int TotalStudents { get; set; }
        public int CurrentUserRank { get; set; }
        public int CurrentUserScore { get; set; }
        public int MaxPossibleScore { get; set; }
        public List<ClassSelectorItem> AvailableClasses { get; set; } = new();
        public List<LeaderboardStudentItem> Students { get; set; } = new();
    }

    public class LeaderboardStudentItem
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int TotalScore { get; set; }
        public int MaxPossibleScore { get; set; }
        public double SuccessRate { get; set; }
        public int CompletedAssignments { get; set; }
        public int TotalAssignments { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
