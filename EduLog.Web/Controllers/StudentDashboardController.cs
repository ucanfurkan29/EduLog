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
    }
}
