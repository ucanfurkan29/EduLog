namespace EduLog.Core.Entities
{
    public class ClassEnrollment
    {
        public int Id { get; set; }

        public int ClassGroupId { get; set; }

        public int UserId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public ClassGroup ClassGroup { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
