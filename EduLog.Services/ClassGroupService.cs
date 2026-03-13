using EduLog.Core.Entities;
using EduLog.Data;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Services
{
    // DTOs for leaderboard and class overview
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int TotalScore { get; set; }
        public int SubmissionCount { get; set; }
    }

    public class WeekSubmissionStat
    {
        public int WeekNumber { get; set; }
        public string Topic { get; set; } = string.Empty;
        public int StudentsSubmittedCount { get; set; }
    }

    public class ClassOverviewStats
    {
        public List<WeekSubmissionStat> WeekStats { get; set; } = new();
        public int UngradedCodeTaskCount { get; set; }
    }

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
        Task<List<LeaderboardEntry>> GetLeaderboardAsync(int classGroupId);
        Task<ClassOverviewStats> GetClassOverviewStatsAsync(int classGroupId);
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

        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(int classGroupId)
        {
            // Get all enrolled students
            var enrollments = await _context.ClassEnrollments
                .Include(e => e.User)
                .Where(e => e.ClassGroupId == classGroupId)
                .ToListAsync();

            // Get all submissions with scores for this class group
            var submissions = await _context.Submissions
                .Where(s => s.ClassGroupId == classGroupId && s.Score.HasValue)
                .ToListAsync();

            var leaderboard = enrollments.Select(e =>
            {
                var studentSubs = submissions.Where(s => s.UserId == e.UserId).ToList();
                return new LeaderboardEntry
                {
                    FullName = e.User.FullName,
                    TotalScore = studentSubs.Sum(s => s.Score ?? 0),
                    SubmissionCount = studentSubs.Count
                };
            })
            .OrderByDescending(l => l.TotalScore)
            .ThenBy(l => l.FullName)
            .ToList();

            // Assign ranks
            for (int i = 0; i < leaderboard.Count; i++)
            {
                leaderboard[i].Rank = i + 1;
            }

            return leaderboard;
        }

        public async Task<ClassOverviewStats> GetClassOverviewStatsAsync(int classGroupId)
        {
            var classGroup = await _context.ClassGroups
                .Include(cg => cg.Syllabus)
                    .ThenInclude(s => s.Weeks.OrderBy(w => w.WeekNumber))
                        .ThenInclude(w => w.Assignments)
                .FirstOrDefaultAsync(cg => cg.Id == classGroupId);

            if (classGroup == null)
                return new ClassOverviewStats();

            var allAssignmentIds = classGroup.Syllabus.Weeks
                .SelectMany(w => w.Assignments)
                .Select(a => a.Id)
                .ToList();

            var submissions = await _context.Submissions
                .Include(s => s.Assignment)
                .Where(s => s.ClassGroupId == classGroupId && allAssignmentIds.Contains(s.AssignmentId))
                .ToListAsync();

            // Per-week stats: how many distinct students submitted at least 1 assignment
            var weekStats = classGroup.Syllabus.Weeks
                .Where(w => w.WeekNumber <= classGroup.CurrentWeek)
                .Select(w =>
                {
                    var weekAssignmentIds = w.Assignments.Select(a => a.Id).ToList();
                    var studentsSubmitted = submissions
                        .Where(s => weekAssignmentIds.Contains(s.AssignmentId))
                        .Select(s => s.UserId)
                        .Distinct()
                        .Count();

                    return new WeekSubmissionStat
                    {
                        WeekNumber = w.WeekNumber,
                        Topic = w.Topic,
                        StudentsSubmittedCount = studentsSubmitted
                    };
                }).ToList();

            // Ungraded CodeTask count
            var ungradedCount = submissions
                .Count(s => s.Assignment.Type == "CodeTask" && !s.Score.HasValue);

            return new ClassOverviewStats
            {
                WeekStats = weekStats,
                UngradedCodeTaskCount = ungradedCount
            };
        }
    }
}
