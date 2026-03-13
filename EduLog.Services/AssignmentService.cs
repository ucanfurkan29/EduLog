using EduLog.Core.Entities;
using EduLog.Data;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Services
{
    public interface IAssignmentService
    {
        Task<Assignment?> GetByIdAsync(int id);
        Task<Assignment?> GetByIdWithQuestionsAsync(int id);
        Task<IEnumerable<Assignment>> GetBySyllabusWeekIdAsync(int syllabusWeekId);
        Task CreateAsync(Assignment assignment);
        Task UpdateAsync(Assignment assignment, List<AssignmentQuestion> questions);
        Task DeleteAsync(int id);
    }

    public class AssignmentService : IAssignmentService
    {
        private readonly AppDbContext _context;

        public AssignmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Assignment?> GetByIdAsync(int id)
        {
            return await _context.Assignments
                .Include(a => a.Week)
                    .ThenInclude(w => w.Syllabus)
                        .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Assignment?> GetByIdWithQuestionsAsync(int id)
        {
            return await _context.Assignments
                .Include(a => a.Questions.OrderBy(q => q.OrderIndex))
                .Include(a => a.Week)
                    .ThenInclude(w => w.Syllabus)
                        .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Assignment>> GetBySyllabusWeekIdAsync(int syllabusWeekId)
        {
            return await _context.Assignments
                .Include(a => a.Questions)
                .Where(a => a.SyllabusWeekId == syllabusWeekId)
                .OrderBy(a => a.Id)
                .ToListAsync();
        }

        public async Task CreateAsync(Assignment assignment)
        {
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Assignment assignment, List<AssignmentQuestion> questions)
        {
            // Remove old questions
            var oldQuestions = await _context.AssignmentQuestions
                .Where(q => q.AssignmentId == assignment.Id)
                .ToListAsync();
            _context.AssignmentQuestions.RemoveRange(oldQuestions);

            // Update assignment
            _context.Assignments.Update(assignment);

            // Add new questions
            foreach (var q in questions)
            {
                q.AssignmentId = assignment.Id;
                _context.AssignmentQuestions.Add(q);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Questions)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment != null)
            {
                // Remove related submissions first (Restrict relationship)
                if (assignment.Submissions.Any())
                {
                    _context.Submissions.RemoveRange(assignment.Submissions);
                }

                // Remove related questions
                if (assignment.Questions.Any())
                {
                    _context.AssignmentQuestions.RemoveRange(assignment.Questions);
                }

                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
