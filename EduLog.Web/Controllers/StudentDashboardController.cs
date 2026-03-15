using EduLog.Data;
using EduLog.Core.Entities;
using EduLog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentDashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var enrollments = await _context.ClassEnrollments
                .Include(e => e.ClassGroup)
                    .ThenInclude(cg => cg.Course)
                .Where(e => e.UserId == user.Id)
                .ToListAsync();

            var model = enrollments.Select(e => new StudentClassViewModel
            {
                ClassGroupId = e.ClassGroup.Id,
                ClassName = e.ClassGroup.Name,
                CourseName = e.ClassGroup.Course.Name,
                CurrentWeek = e.ClassGroup.CurrentWeek
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> ClassDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Verify enrollment
            var enrollment = await _context.ClassEnrollments
                .FirstOrDefaultAsync(e => e.ClassGroupId == id && e.UserId == user.Id);

            if (enrollment == null)
            {
                TempData["Error"] = "Bu sınıfa erişim yetkiniz yok.";
                return RedirectToAction(nameof(Index));
            }

            var classGroup = await _context.ClassGroups
                .Include(cg => cg.Course)
                .Include(cg => cg.Syllabus)
                    .ThenInclude(s => s.Weeks.OrderBy(w => w.WeekNumber))
                        .ThenInclude(w => w.Assignments)
                .Include(cg => cg.Enrollments)
                .FirstOrDefaultAsync(cg => cg.Id == id);

            if (classGroup == null)
            {
                TempData["Error"] = "Sınıf bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Get all scored submissions for this class
            var allSubmissions = await _context.Submissions
                .Where(s => s.ClassGroupId == id && s.Score.HasValue)
                .ToListAsync();

            // Current user's total score
            var myTotalScore = allSubmissions
                .Where(s => s.UserId == user.Id)
                .Sum(s => s.Score ?? 0);

            // Calculate class rank
            var allStudentIds = classGroup.Enrollments.Select(e => e.UserId).ToList();
            var studentScores = allStudentIds.Select(sid => new
            {
                UserId = sid,
                TotalScore = allSubmissions.Where(s => s.UserId == sid).Sum(s => s.Score ?? 0)
            })
            .OrderByDescending(x => x.TotalScore)
            .ToList();

            var classRank = studentScores.FindIndex(x => x.UserId == user.Id) + 1;
            if (classRank == 0) classRank = studentScores.Count + 1; // not found edge case

            // Per-week score for current user
            var openWeeks = classGroup.Syllabus.Weeks
                .Where(w => w.WeekNumber <= classGroup.CurrentWeek)
                .Select(w =>
                {
                    var weekAssignmentIds = w.Assignments.Select(a => a.Id).ToList();
                    var weekScore = allSubmissions
                        .Where(s => s.UserId == user.Id && weekAssignmentIds.Contains(s.AssignmentId))
                        .Sum(s => s.Score ?? 0);

                    return new StudentWeekItemViewModel
                    {
                        WeekNumber = w.WeekNumber,
                        Topic = w.Topic,
                        WeekScore = weekScore
                    };
                }).ToList();

            var model = new StudentClassDetailViewModel
            {
                ClassGroupId = classGroup.Id,
                ClassName = classGroup.Name,
                CourseName = classGroup.Course.Name,
                CurrentWeek = classGroup.CurrentWeek,
                TotalScore = myTotalScore,
                ClassRank = classRank,
                OpenWeeks = openWeeks
            };

            return View(model);
        }

        public async Task<IActionResult> WeekDetail(int classGroupId, int weekNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Verify enrollment
            var enrollment = await _context.ClassEnrollments
                .FirstOrDefaultAsync(e => e.ClassGroupId == classGroupId && e.UserId == user.Id);

            if (enrollment == null)
            {
                TempData["Error"] = "Bu sınıfa erişim yetkiniz yok.";
                return RedirectToAction(nameof(Index));
            }

            var classGroup = await _context.ClassGroups
                .Include(cg => cg.Course)
                .Include(cg => cg.Syllabus)
                    .ThenInclude(s => s.Weeks.OrderBy(w => w.WeekNumber))
                        .ThenInclude(w => w.Resources)
                .Include(cg => cg.Syllabus)
                    .ThenInclude(s => s.Weeks.OrderBy(w => w.WeekNumber))
                        .ThenInclude(w => w.Assignments)
                .FirstOrDefaultAsync(cg => cg.Id == classGroupId);

            if (classGroup == null)
            {
                TempData["Error"] = "Sınıf bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Check if week is open
            if (weekNumber > classGroup.CurrentWeek)
            {
                TempData["Error"] = "Bu ders henüz açılmamış.";
                return RedirectToAction(nameof(ClassDetail), new { id = classGroupId });
            }

            var week = classGroup.Syllabus.Weeks.FirstOrDefault(w => w.WeekNumber == weekNumber);
            if (week == null)
            {
                TempData["Error"] = "Ders bulunamadı.";
                return RedirectToAction(nameof(ClassDetail), new { id = classGroupId });
            }

            // Get user's submissions for this week's assignments
            var assignmentIds = week.Assignments.Select(a => a.Id).ToList();
            var userSubmissions = await _context.Submissions
                .Where(s => assignmentIds.Contains(s.AssignmentId) && s.UserId == user.Id)
                .ToListAsync();

            var model = new StudentWeekDetailViewModel
            {
                ClassGroupId = classGroup.Id,
                ClassName = classGroup.Name,
                CourseName = classGroup.Course.Name,
                WeekNumber = week.WeekNumber,
                Topic = week.Topic,
                Notes = week.Notes,
                Examples = week.Examples,
                Resources = week.Resources.Select(r => new StudentResourceViewModel
                {
                    FileName = r.FileName,
                    FilePath = r.FilePath,
                    ResourceType = r.ResourceType
                }).ToList(),
                Assignments = week.Assignments.Select(a =>
                {
                    var sub = userSubmissions.FirstOrDefault(s => s.AssignmentId == a.Id);
                    return new StudentAssignmentViewModel
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Description = a.Description,
                        Type = a.Type,
                        MaxScore = a.MaxScore,
                        DueDate = a.DueDate,
                        HasSubmission = sub != null,
                        SubmissionId = sub?.Id,
                        Score = sub?.Score,
                        ExpectedBehavior = a.ExpectedBehavior
                    };
                }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Get all enrollments with course info
            var enrollments = await _context.ClassEnrollments
                .Include(e => e.ClassGroup)
                    .ThenInclude(cg => cg.Course)
                .Include(e => e.ClassGroup)
                    .ThenInclude(cg => cg.Syllabus)
                        .ThenInclude(s => s.Weeks.OrderBy(w => w.WeekNumber))
                            .ThenInclude(w => w.Assignments)
                .Include(e => e.ClassGroup)
                    .ThenInclude(cg => cg.Enrollments)
                .Where(e => e.UserId == user.Id)
                .ToListAsync();

            int totalScore = 0;
            int completedAssignmentCount = 0;
            int totalAssignmentCount = 0;
            int totalMaxScore = 0;
            int totalEarnedScore = 0;
            var courseStats = new List<CourseStatViewModel>();
            var weeklyScores = new List<WeeklyScoreViewModel>();

            foreach (var enrollment in enrollments)
            {
                var cg = enrollment.ClassGroup;
                var openWeeks = cg.Syllabus.Weeks
                    .Where(w => w.WeekNumber <= cg.CurrentWeek)
                    .ToList();

                var openAssignmentIds = openWeeks
                    .SelectMany(w => w.Assignments)
                    .Select(a => a.Id)
                    .ToList();

                var openAssignments = openWeeks
                    .SelectMany(w => w.Assignments)
                    .ToList();

                // Submissions for THIS class
                var classSubs = await _context.Submissions
                    .Where(s => s.ClassGroupId == cg.Id && s.Score.HasValue)
                    .ToListAsync();

                var mySubs = classSubs.Where(s => s.UserId == user.Id).ToList();
                var myClassScore = mySubs.Sum(s => s.Score ?? 0);
                var myCompletedCount = mySubs.Count;
                var classAssignmentCount = openAssignments.Count;
                var classMaxScore = openAssignments.Sum(a => a.MaxScore);

                totalScore += myClassScore;
                completedAssignmentCount += myCompletedCount;
                totalAssignmentCount += classAssignmentCount;
                totalMaxScore += classMaxScore;
                totalEarnedScore += myClassScore;

                // Calculate rank within class
                var studentScores = cg.Enrollments.Select(e => new
                {
                    UserId = e.UserId,
                    Score = classSubs.Where(s => s.UserId == e.UserId).Sum(s => s.Score ?? 0)
                })
                .OrderByDescending(x => x.Score)
                .ToList();

                var rank = studentScores.FindIndex(x => x.UserId == user.Id) + 1;
                if (rank == 0) rank = studentScores.Count + 1;

                double successRate = classMaxScore > 0
                    ? Math.Round((double)myClassScore / classMaxScore * 100, 1)
                    : 0;

                courseStats.Add(new CourseStatViewModel
                {
                    CourseName = cg.Course.Name,
                    ClassName = cg.Name,
                    ClassGroupId = cg.Id,
                    TotalScore = myClassScore,
                    MaxPossibleScore = classMaxScore,
                    CompletedCount = myCompletedCount,
                    TotalCount = classAssignmentCount,
                    SuccessRate = successRate,
                    Rank = rank
                });

                // Weekly scores for chart
                foreach (var week in openWeeks)
                {
                    var weekAssignmentIds = week.Assignments.Select(a => a.Id).ToList();
                    var weekScore = mySubs
                        .Where(s => weekAssignmentIds.Contains(s.AssignmentId))
                        .Sum(s => s.Score ?? 0);

                    weeklyScores.Add(new WeeklyScoreViewModel
                    {
                        Label = $"{cg.Course.Name} - Hafta {week.WeekNumber}",
                        Score = weekScore
                    });
                }
            }

            double overallSuccessRate = totalMaxScore > 0
                ? Math.Round((double)totalEarnedScore / totalMaxScore * 100, 1)
                : 0;

            var model = new StudentProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                JoinedAt = enrollments.Any() ? enrollments.Min(e => e.JoinedAt) : DateTime.UtcNow,
                TotalScore = totalScore,
                CompletedAssignmentCount = completedAssignmentCount,
                TotalAssignmentCount = totalAssignmentCount,
                EnrolledClassCount = enrollments.Count,
                OverallSuccessRate = overallSuccessRate,
                CourseStats = courseStats,
                WeeklyScores = weeklyScores
            };

            return View(model);
        }

        public async Task<IActionResult> MyAssignments(int? classGroupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var enrollments = await _context.ClassEnrollments
                .Include(e => e.ClassGroup)
                    .ThenInclude(cg => cg.Course)
                .Include(e => e.ClassGroup)
                    .ThenInclude(cg => cg.Syllabus)
                        .ThenInclude(s => s.Weeks.OrderBy(w => w.WeekNumber))
                            .ThenInclude(w => w.Assignments)
                .Where(e => e.UserId == user.Id)
                .ToListAsync();

            // Build available classes list
            var availableClasses = enrollments.Select(e => new ClassSelectorItem
            {
                ClassGroupId = e.ClassGroup.Id,
                CourseName = e.ClassGroup.Course.Name,
                ClassName = e.ClassGroup.Name
            }).ToList();

            // Filter enrollments if a class is selected
            var filteredEnrollments = classGroupId.HasValue
                ? enrollments.Where(e => e.ClassGroup.Id == classGroupId.Value).ToList()
                : enrollments;

            var classGroups = new List<MyAssignmentClassGroup>();
            int totalCount = 0;
            int submittedCount = 0;
            int totalMaxScore = 0;
            int totalEarnedScore = 0;

            foreach (var enrollment in filteredEnrollments)
            {
                var cg = enrollment.ClassGroup;
                var openWeeks = cg.Syllabus.Weeks
                    .Where(w => w.WeekNumber <= cg.CurrentWeek)
                    .ToList();

                var openAssignments = openWeeks
                    .SelectMany(w => w.Assignments.Select(a => new { Assignment = a, Week = w }))
                    .ToList();

                if (!openAssignments.Any()) continue;

                var assignmentIds = openAssignments.Select(x => x.Assignment.Id).ToList();
                var userSubmissions = await _context.Submissions
                    .Where(s => assignmentIds.Contains(s.AssignmentId) && s.UserId == user.Id)
                    .ToListAsync();

                var items = openAssignments.Select(x =>
                {
                    var sub = userSubmissions.FirstOrDefault(s => s.AssignmentId == x.Assignment.Id);
                    return new MyAssignmentItemViewModel
                    {
                        Id = x.Assignment.Id,
                        Title = x.Assignment.Title,
                        Description = x.Assignment.Description,
                        Type = x.Assignment.Type,
                        MaxScore = x.Assignment.MaxScore,
                        WeekNumber = x.Week.WeekNumber,
                        WeekTopic = x.Week.Topic,
                        DueDate = x.Assignment.DueDate,
                        HasSubmission = sub != null,
                        SubmissionId = sub?.Id,
                        Score = sub?.Score,
                        ClassGroupId = cg.Id
                    };
                })
                .OrderBy(a => a.HasSubmission)
                .ThenBy(a => a.DueDate ?? DateTime.MaxValue)
                .ThenBy(a => a.WeekNumber)
                .ToList();

                totalCount += items.Count;
                submittedCount += items.Count(i => i.HasSubmission);
                totalMaxScore += openAssignments.Sum(x => x.Assignment.MaxScore);
                totalEarnedScore += userSubmissions.Where(s => s.Score.HasValue).Sum(s => s.Score ?? 0);

                classGroups.Add(new MyAssignmentClassGroup
                {
                    ClassGroupId = cg.Id,
                    ClassName = cg.Name,
                    CourseName = cg.Course.Name,
                    Assignments = items
                });
            }

            double successRate = totalMaxScore > 0
                ? Math.Round((double)totalEarnedScore / totalMaxScore * 100, 1)
                : 0;

            var model = new MyAssignmentsViewModel
            {
                TotalAssignmentCount = totalCount,
                SubmittedCount = submittedCount,
                PendingCount = totalCount - submittedCount,
                OverallSuccessRate = successRate,
                SelectedClassGroupId = classGroupId,
                AvailableClasses = availableClasses,
                ClassGroups = classGroups
            };

            return View(model);
        }

        public async Task<IActionResult> Leaderboard(int? classGroupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Get all enrolled classes
            var enrollments = await _context.ClassEnrollments
                .Include(e => e.ClassGroup)
                    .ThenInclude(cg => cg.Course)
                .Where(e => e.UserId == user.Id)
                .ToListAsync();

            if (!enrollments.Any())
            {
                return View(new LeaderboardViewModel
                {
                    CurrentUserId = user.Id,
                    AvailableClasses = new List<ClassSelectorItem>()
                });
            }

            var availableClasses = enrollments.Select(e => new ClassSelectorItem
            {
                ClassGroupId = e.ClassGroup.Id,
                CourseName = e.ClassGroup.Course.Name,
                ClassName = e.ClassGroup.Name
            }).ToList();

            // Use first class if none selected
            var selectedId = classGroupId ?? availableClasses.First().ClassGroupId;

            // Load the selected class group with full data
            var classGroup = await _context.ClassGroups
                .Include(cg => cg.Course)
                .Include(cg => cg.Syllabus)
                    .ThenInclude(s => s.Weeks.OrderBy(w => w.WeekNumber))
                        .ThenInclude(w => w.Assignments)
                .Include(cg => cg.Enrollments)
                    .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(cg => cg.Id == selectedId);

            if (classGroup == null)
            {
                TempData["Error"] = "Sınıf bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Get open assignments
            var openWeeks = classGroup.Syllabus.Weeks
                .Where(w => w.WeekNumber <= classGroup.CurrentWeek)
                .ToList();

            var openAssignments = openWeeks
                .SelectMany(w => w.Assignments)
                .ToList();

            var totalAssignments = openAssignments.Count;
            var maxPossibleScore = openAssignments.Sum(a => a.MaxScore);
            var openAssignmentIds = openAssignments.Select(a => a.Id).ToList();

            // Get all submissions for this class
            var allSubmissions = await _context.Submissions
                .Where(s => s.ClassGroupId == selectedId && openAssignmentIds.Contains(s.AssignmentId))
                .ToListAsync();

            // Build student list with scores
            var studentItems = classGroup.Enrollments
                .Select(e =>
                {
                    var studentSubs = allSubmissions.Where(s => s.UserId == e.UserId).ToList();
                    var totalScore = studentSubs.Where(s => s.Score.HasValue).Sum(s => s.Score ?? 0);
                    var completedCount = studentSubs.Count;
                    double successRate = maxPossibleScore > 0
                        ? Math.Round((double)totalScore / maxPossibleScore * 100, 1)
                        : 0;

                    return new LeaderboardStudentItem
                    {
                        UserId = e.UserId,
                        FullName = e.User.FullName,
                        TotalScore = totalScore,
                        MaxPossibleScore = maxPossibleScore,
                        SuccessRate = successRate,
                        CompletedAssignments = completedCount,
                        TotalAssignments = totalAssignments,
                        IsCurrentUser = e.UserId == user.Id
                    };
                })
                .OrderByDescending(s => s.TotalScore)
                .ThenBy(s => s.FullName) // Eşit puanda alfabetik
                .ToList();

            // Assign ranks
            for (int i = 0; i < studentItems.Count; i++)
            {
                studentItems[i].Rank = i + 1;
            }

            var currentStudent = studentItems.FirstOrDefault(s => s.IsCurrentUser);

            var model = new LeaderboardViewModel
            {
                SelectedClassGroupId = selectedId,
                SelectedClassName = classGroup.Name,
                SelectedCourseName = classGroup.Course.Name,
                CurrentUserId = user.Id,
                TotalStudents = studentItems.Count,
                CurrentUserRank = currentStudent?.Rank ?? 0,
                CurrentUserScore = currentStudent?.TotalScore ?? 0,
                MaxPossibleScore = maxPossibleScore,
                AvailableClasses = availableClasses,
                Students = studentItems
            };

            return View(model);
        }
    }
}
