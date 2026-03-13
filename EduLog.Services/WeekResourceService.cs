using EduLog.Core.Entities;
using EduLog.Data;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Services
{
    public interface IWeekResourceService
    {
        Task<IEnumerable<WeekResource>> GetByWeekIdAsync(int syllabusWeekId);
        Task<WeekResource?> GetByIdAsync(int id);
        Task CreateAsync(WeekResource resource);
        Task DeleteAsync(int id);
    }

    public class WeekResourceService : IWeekResourceService
    {
        private readonly AppDbContext _context;

        public WeekResourceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WeekResource>> GetByWeekIdAsync(int syllabusWeekId)
        {
            return await _context.WeekResources
                .Where(r => r.SyllabusWeekId == syllabusWeekId)
                .OrderBy(r => r.FileName)
                .ToListAsync();
        }

        public async Task<WeekResource?> GetByIdAsync(int id)
        {
            return await _context.WeekResources.FindAsync(id);
        }

        public async Task CreateAsync(WeekResource resource)
        {
            _context.WeekResources.Add(resource);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var resource = await _context.WeekResources.FindAsync(id);
            if (resource != null)
            {
                _context.WeekResources.Remove(resource);
                await _context.SaveChangesAsync();
            }
        }
    }
}
