using EduLog.Core.Entities;
using EduLog.Data;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Services
{
    public interface IClassGroupService
    {
        Task<ClassGroup?> GetByJoinCodeAsync(string joinCode);
        Task<IEnumerable<ClassGroup>> GetAllAsync();
        Task<ClassGroup?> GetByIdAsync(int id);
        Task CreateAsync(ClassGroup classGroup);
        Task DeleteAsync(int id);
        Task<bool> AdvanceWeekAsync(int id);
        Task<IEnumerable<ClassEnrollment>> GetEnrollmentsAsync(int classGroupId);
        Task<string> GenerateUniqueJoinCodeAsync();
    }

    public class ClassGroupService : IClassGroupService
    {
        private readonly AppDbContext _context;

        public ClassGroupService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ClassGroup?> GetByJoinCodeAsync(string joinCode)
        {
            return await _context.ClassGroups
                .Include(cg => cg.Course)
                .FirstOrDefaultAsync(cg => cg.JoinCode == joinCode);
        }

        public async Task<IEnumerable<ClassGroup>> GetAllAsync()
        {
            return await _context.ClassGroups
                .Include(cg => cg.Course)
                .Include(cg => cg.Syllabus)
                .Include(cg => cg.Enrollments)
                .OrderByDescending(cg => cg.CreatedAt)
                .ToListAsync();
        }

        public async Task<ClassGroup?> GetByIdAsync(int id)
        {
            return await _context.ClassGroups
                .Include(cg => cg.Course)
                .Include(cg => cg.Syllabus)
                    .ThenInclude(s => s.Weeks.OrderBy(w => w.WeekNumber))
                        .ThenInclude(w => w.Resources)
                .Include(cg => cg.Syllabus)
                    .ThenInclude(s => s.Weeks.OrderBy(w => w.WeekNumber))
                        .ThenInclude(w => w.Assignments)
                .Include(cg => cg.Enrollments)
                    .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(cg => cg.Id == id);
        }

        public async Task CreateAsync(ClassGroup classGroup)
        {
            classGroup.JoinCode = await GenerateUniqueJoinCodeAsync();
            classGroup.CurrentWeek = 0;
            classGroup.CreatedAt = DateTime.UtcNow;
            _context.ClassGroups.Add(classGroup);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var classGroup = await _context.ClassGroups.FindAsync(id);
            if (classGroup != null)
            {
                _context.ClassGroups.Remove(classGroup);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> AdvanceWeekAsync(int id)
        {
            var classGroup = await _context.ClassGroups
                .Include(cg => cg.Syllabus)
                    .ThenInclude(s => s.Weeks)
                .FirstOrDefaultAsync(cg => cg.Id == id);

            if (classGroup == null) return false;

            int maxWeek = classGroup.Syllabus.Weeks.Count;
            if (classGroup.CurrentWeek >= maxWeek) return false;

            classGroup.CurrentWeek++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ClassEnrollment>> GetEnrollmentsAsync(int classGroupId)
        {
            return await _context.ClassEnrollments
                .Include(e => e.User)
                .Where(e => e.ClassGroupId == classGroupId)
                .OrderBy(e => e.User.FullName)
                .ToListAsync();
        }

        public async Task<string> GenerateUniqueJoinCodeAsync()
        {
            string code;
            do
            {
                code = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            }
            while (await _context.ClassGroups.AnyAsync(cg => cg.JoinCode == code));

            return code;
        }
    }
}
