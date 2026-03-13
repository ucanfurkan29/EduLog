using EduLog.Core.Entities;
using EduLog.Data;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Services
{
    public interface ISyllabusService
    {
        Task<IEnumerable<Syllabus>> GetByCourseIdAsync(int courseId);
        Task<Syllabus?> GetByIdAsync(int id);
        Task<Syllabus?> GetByIdWithWeeksAsync(int id);
        Task CreateAsync(Syllabus syllabus);
        Task UpdateAsync(Syllabus syllabus);
        Task DeleteAsync(int id);
    }

    public class SyllabusService : ISyllabusService
    {
        private readonly AppDbContext _context;

        public SyllabusService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Syllabus>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Syllabi
                .Include(s => s.Course)
                .Include(s => s.Weeks)
                .Where(s => s.CourseId == courseId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Syllabus?> GetByIdAsync(int id)
        {
            return await _context.Syllabi
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Syllabus?> GetByIdWithWeeksAsync(int id)
        {
            return await _context.Syllabi
                .Include(s => s.Course)
                .Include(s => s.Weeks)
                    .ThenInclude(w => w.Resources)
                .Include(s => s.Weeks)
                    .ThenInclude(w => w.Assignments)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task CreateAsync(Syllabus syllabus)
        {
            _context.Syllabi.Add(syllabus);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Syllabus syllabus)
        {
            _context.Syllabi.Update(syllabus);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var syllabus = await _context.Syllabi.FindAsync(id);
            if (syllabus != null)
            {
                _context.Syllabi.Remove(syllabus);
                await _context.SaveChangesAsync();
            }
        }
    }
}
