using System.ComponentModel.DataAnnotations;

namespace EduLog.Core.Entities
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public ICollection<Syllabus> Syllabi { get; set; } = new List<Syllabus>();
        public ICollection<ClassGroup> ClassGroups { get; set; } = new List<ClassGroup>();
    }
}
