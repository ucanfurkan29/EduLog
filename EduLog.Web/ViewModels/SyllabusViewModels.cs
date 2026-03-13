using System.ComponentModel.DataAnnotations;

namespace EduLog.Web.ViewModels
{
    public class CreateSyllabusViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Müfredat başlığı gereklidir.")]
        [MaxLength(300)]
        [Display(Name = "Müfredat Başlığı")]
        public string Title { get; set; } = string.Empty;
    }

    public class EditSyllabusViewModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Müfredat başlığı gereklidir.")]
        [MaxLength(300)]
        [Display(Name = "Müfredat Başlığı")]
        public string Title { get; set; } = string.Empty;
    }
}
