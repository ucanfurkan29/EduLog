using Microsoft.AspNetCore.Identity;

namespace EduLog.Core.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
    }
}
