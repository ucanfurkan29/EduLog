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
                .FirstOrDefaultAsync(cg => cg.Id == id);

            if (classGroup == null)
            {
                TempData["Error"] = "Sınıf bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var openWeeks = classGroup.Syllabus.Weeks
                .Where(w => w.WeekNumber <= classGroup.CurrentWeek)
                .Select(w => new StudentWeekItemViewModel
                {
                    WeekNumber = w.WeekNumber,
                    Topic = w.Topic
                }).ToList();

            var model = new StudentClassDetailViewModel
            {
                ClassGroupId = classGroup.Id,
                ClassName = classGroup.Name,
                CourseName = classGroup.Course.Name,
                CurrentWeek = classGroup.CurrentWeek,
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
                TempData["Error"] = "Bu hafta henüz açılmamış.";
                return RedirectToAction(nameof(ClassDetail), new { id = classGroupId });
            }

            var week = classGroup.Syllabus.Weeks.FirstOrDefault(w => w.WeekNumber == weekNumber);
            if (week == null)
            {
                TempData["Error"] = "Hafta bulunamadı.";
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
                        Score = sub?.Score
                    };
                }).ToList()
            };

            return View(model);
        }
    }
}
