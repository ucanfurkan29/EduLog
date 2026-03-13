using System.ComponentModel.DataAnnotations;
using EduLog.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace EduLog.Web.ViewModels
{
    public class CreateSyllabusWeekViewModel
    {
        public int SyllabusId { get; set; }
        public string SyllabusTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hafta numarası gereklidir.")]
        [Range(1, 52, ErrorMessage = "Hafta numarası 1 ile 52 arasında olmalıdır.")]
        [Display(Name = "Hafta No")]
        public int WeekNumber { get; set; }

        [Required(ErrorMessage = "Konu gereklidir.")]
        [MaxLength(300)]
        [Display(Name = "Konu")]
        public string Topic { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "Örnekler")]
        public string? Examples { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }
    }

    public class EditSyllabusWeekViewModel
    {
        public int Id { get; set; }
        public int SyllabusId { get; set; }
        public string SyllabusTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hafta numarası gereklidir.")]
        [Range(1, 52, ErrorMessage = "Hafta numarası 1 ile 52 arasında olmalıdır.")]
        [Display(Name = "Hafta No")]
        public int WeekNumber { get; set; }

        [Required(ErrorMessage = "Konu gereklidir.")]
        [MaxLength(300)]
        [Display(Name = "Konu")]
        public string Topic { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "Örnekler")]
        public string? Examples { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }
    }

    public class SyllabusWeekDetailViewModel
    {
        public SyllabusWeek Week { get; set; } = null!;
        public string SyllabusTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int SyllabusId { get; set; }
        public IEnumerable<WeekResource> Resources { get; set; } = new List<WeekResource>();
        public IEnumerable<Assignment> Assignments { get; set; } = new List<Assignment>();
    }

    public class FileUploadViewModel
    {
        public int SyllabusWeekId { get; set; }
        public int SyllabusId { get; set; }

        [Required(ErrorMessage = "Dosya seçiniz.")]
        [Display(Name = "Dosya")]
        public IFormFile File { get; set; } = null!;
    }
}
