using EduLog.Core.Entities;
using EduLog.Data;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Services
{
    public interface ISubmissionService
    {
        Task<Submission?> GetByIdAsync(int id);
        Task<Submission?> GetByAssignmentAndUserAsync(int assignmentId, int userId);
        Task<IEnumerable<Submission>> GetByClassGroupIdAsync(int classGroupId);
        Task SubmitAsync(Submission submission);
        Task GradeAsync(int submissionId, int score, string? note);
        int AutoGradeQuiz(string submittedAnswers, ICollection<AssignmentQuestion> questions, int maxScore);
    }

    public class SubmissionService : ISubmissionService
    {
        private readonly AppDbContext _context;

        public SubmissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Submission?> GetByIdAsync(int id)
        {
            return await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Questions)
                .Include(s => s.User)
                .Include(s => s.ClassGroup)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Submission?> GetByAssignmentAndUserAsync(int assignmentId, int userId)
        {
            return await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.UserId == userId);
        }

        public async Task<IEnumerable<Submission>> GetByClassGroupIdAsync(int classGroupId)
        {
            return await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.User)
                .Where(s => s.ClassGroupId == classGroupId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task SubmitAsync(Submission submission)
        {
            submission.SubmittedAt = DateTime.UtcNow;
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();
        }

        public async Task GradeAsync(int submissionId, int score, string? note)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission != null)
            {
                submission.Score = score;
                submission.InstructorNote = note;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Otomatik quiz puanlama. submittedAnswers formatı: "A,B,C,D,A" (virgülle ayrılmış)
        /// (doğru sayısı / toplam soru) × MaxScore
        /// </summary>
        public int AutoGradeQuiz(string submittedAnswers, ICollection<AssignmentQuestion> questions, int maxScore)
        {
            var answers = submittedAnswers.Split(',');
            var orderedQuestions = questions.OrderBy(q => q.OrderIndex).ToList();
            int correctCount = 0;

            for (int i = 0; i < orderedQuestions.Count && i < answers.Length; i++)
            {
                if (string.Equals(answers[i].Trim(), orderedQuestions[i].CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    correctCount++;
                }
            }

            int totalQuestions = orderedQuestions.Count;
            if (totalQuestions == 0) return 0;

            return (int)Math.Round((double)correctCount / totalQuestions * maxScore);
        }
    }
}
