using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduLog.Web.ViewModels
{
    public class CreateClassGroupViewModel
    {
        [Required(ErrorMessage = "Sınıf adı gereklidir.")]
        [MaxLength(200)]
        [Display(Name = "Sınıf Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ders seçimi gereklidir.")]
        [Display(Name = "Ders")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Müfredat seçimi gereklidir.")]
        [Display(Name = "Müfredat")]
        public int SyllabusId { get; set; }

        public List<SelectListItem> Courses { get; set; } = new();
        public List<SelectListItem> Syllabi { get; set; } = new();
    }

    public class ClassGroupIndexItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SyllabusTitle { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int CurrentWeek { get; set; }
        public string JoinCode { get; set; } = string.Empty;
    }

    public class ClassGroupDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SyllabusTitle { get; set; } = string.Empty;
        public int CurrentWeek { get; set; }
        public int MaxWeek { get; set; }
        public List<StudentItemViewModel> Students { get; set; } = new();
        public List<SubmissionListItemViewModel> Submissions { get; set; } = new();
    }

    public class StudentItemViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
