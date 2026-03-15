using System.ComponentModel.DataAnnotations;

namespace EduLog.Web.ViewModels
{
    public class CreateCourseViewModel
    {
        [Required(ErrorMessage = "Ders adı gereklidir.")]
        [MaxLength(200)]
        [Display(Name = "Ders Adı")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Programlama Dili")]
        public string ProgrammingLanguage { get; set; } = "csharp";
    }

    public class EditCourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ders adı gereklidir.")]
        [MaxLength(200)]
        [Display(Name = "Ders Adı")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Programlama Dili")]
        public string ProgrammingLanguage { get; set; } = "csharp";
    }
}
