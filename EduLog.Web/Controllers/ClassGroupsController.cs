using EduLog.Core.Entities;
using EduLog.Data;
using EduLog.Services;
using EduLog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class ClassGroupsController : Controller
    {
        private readonly IClassGroupService _classGroupService;
        private readonly ICourseService _courseService;
        private readonly ISyllabusService _syllabusService;
        private readonly ISubmissionService _submissionService;
        private readonly IAnthropicService _anthropicService;
        private readonly GeminiService _geminiService;
        private readonly AppDbContext _context;

        public ClassGroupsController(
            IClassGroupService classGroupService,
            ICourseService courseService,
            ISyllabusService syllabusService,
            ISubmissionService submissionService,
            IAnthropicService anthropicService,
            GeminiService geminiService,
            AppDbContext context)
        {
            _classGroupService = classGroupService;
            _courseService = courseService;
            _syllabusService = syllabusService;
            _submissionService = submissionService;
            _anthropicService = anthropicService;
            _geminiService = geminiService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var classGroups = await _classGroupService.GetAllAsync();
            var items = classGroups.Select(cg => new ClassGroupIndexItemViewModel
            {
                Id = cg.Id,
                Name = cg.Name,
                CourseName = cg.Course.Name,
                SyllabusTitle = cg.Syllabus?.Title ?? "-",
                StudentCount = cg.Enrollments.Count,
                CurrentWeek = cg.CurrentWeek,
                JoinCode = cg.JoinCode
            }).ToList();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            var model = new CreateClassGroupViewModel
            {
                Courses = courses.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClassGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var courses = await _courseService.GetAllCoursesAsync();
                model.Courses = courses.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

                if (model.CourseId > 0)
                {
                    var syllabi = await _syllabusService.GetByCourseIdAsync(model.CourseId);
                    model.Syllabi = syllabi.Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Title
                    }).ToList();
                }

                return View(model);
            }

            var classGroup = new ClassGroup
            {
                Name = model.Name,
                CourseId = model.CourseId,
                SyllabusId = model.SyllabusId
            };

            await _classGroupService.CreateAsync(classGroup);
            TempData["Success"] = $"'{classGroup.Name}' sınıfı başarıyla oluşturuldu. Katılım Kodu: {classGroup.JoinCode}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            var classGroup = await _classGroupService.GetByIdAsync(id);
            if (classGroup == null)
            {
                TempData["Error"] = "Sınıf bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var submissions = await _submissionService.GetByClassGroupIdAsync(id);
            var leaderboard = await _classGroupService.GetLeaderboardAsync(id);
            var overviewStats = await _classGroupService.GetClassOverviewStatsAsync(id);

            var model = new ClassGroupDetailViewModel
            {
                Id = classGroup.Id,
                Name = classGroup.Name,
                JoinCode = classGroup.JoinCode,
                CourseName = classGroup.Course.Name,
                SyllabusTitle = classGroup.Syllabus.Title,
                CurrentWeek = classGroup.CurrentWeek,
                MaxWeek = classGroup.Syllabus.Weeks.Count,
                Students = classGroup.Enrollments.Select(e => new StudentItemViewModel
                {
                    FullName = e.User.FullName,
                    Email = e.User.Email ?? "",
                    JoinedAt = e.JoinedAt
                }).OrderBy(s => s.FullName).ToList(),
                Submissions = submissions.Select(s => new SubmissionListItemViewModel
                {
                    Id = s.Id,
                    AssignmentId = s.AssignmentId,
                    StudentName = s.User.FullName,
                    AssignmentTitle = s.Assignment.Title,
                    AssignmentType = s.Assignment.Type,
                    ContentPreview = s.Content.Length > 100 ? s.Content.Substring(0, 100) + "..." : s.Content,
                    Score = s.Score,
                    MaxScore = s.Assignment.MaxScore,
                    SubmittedAt = s.SubmittedAt
                }).ToList(),
                Leaderboard = leaderboard.Select(l => new LeaderboardEntryViewModel
                {
                    Rank = l.Rank,
                    FullName = l.FullName,
                    TotalScore = l.TotalScore,
                    SubmissionCount = l.SubmissionCount
                }).ToList(),
                WeekStats = overviewStats.WeekStats.Select(w => new WeekSubmissionStatViewModel
                {
                    WeekNumber = w.WeekNumber,
                    Topic = w.Topic,
                    StudentsSubmittedCount = w.StudentsSubmittedCount
                }).ToList(),
                UngradedCodeTaskCount = overviewStats.UngradedCodeTaskCount
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceWeek(int id)
        {
            var result = await _classGroupService.AdvanceWeekAsync(id);
            if (!result)
            {
                TempData["Error"] = "Ders açılamadı. Tüm dersler zaten açılmış olabilir.";
            }
            else
            {
                TempData["Success"] = "Sonraki ders başarıyla açıldı.";
            }

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var classGroup = await _classGroupService.GetByIdAsync(id);
            if (classGroup == null)
            {
                TempData["Error"] = "Sınıf bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            await _classGroupService.DeleteAsync(id);
            TempData["Success"] = $"'{classGroup.Name}' sınıfı silindi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetSyllabiForCourse(int courseId)
        {
            var syllabi = await _syllabusService.GetByCourseIdAsync(courseId);
            var items = syllabi.Select(s => new { value = s.Id, text = s.Title });
            return Json(items);
        }

        [HttpGet]
        public async Task<IActionResult> GradeSubmission(int id)
        {
            var submission = await _submissionService.GetByIdAsync(id);
            if (submission == null)
            {
                TempData["Error"] = "Teslim bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Get programming language from the course chain
            var week = await _context.SyllabusWeeks
                .Include(w => w.Syllabus).ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(w => w.Id == submission.Assignment.SyllabusWeekId);
            var programmingLanguage = week?.Syllabus?.Course?.ProgrammingLanguage ?? "csharp";

            var model = new GradeSubmissionViewModel
            {
                SubmissionId = submission.Id,
                StudentName = submission.User.FullName,
                AssignmentTitle = submission.Assignment.Title,
                Content = submission.Content,
                MaxScore = submission.Assignment.MaxScore,
                SubmittedAt = submission.SubmittedAt,
                ClassGroupId = submission.ClassGroupId,
                Score = submission.Score ?? 0,
                InstructorNote = submission.InstructorNote,
                AssignmentType = submission.Assignment.Type,
                ProgrammingLanguage = programmingLanguage,
                AssignmentDescription = submission.Assignment.Description,
                ExpectedBehavior = submission.Assignment.ExpectedBehavior
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AIReviewSubmission(int submissionId, string provider = "anthropic")
        {
            var submission = await _submissionService.GetByIdAsync(submissionId);
            if (submission == null)
                return Json(new { success = false, error = "Teslim bulunamadı." });

            var assignment = submission.Assignment;
            var week = await _context.SyllabusWeeks
                .Include(w => w.Syllabus).ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(w => w.Id == assignment.SyllabusWeekId);

            var topic = week?.Topic ?? "Bilinmiyor";
            var taskDescription = $"{assignment.Description ?? assignment.Title}\nBeklenen Davranış: {assignment.ExpectedBehavior ?? "Belirtilmemiş"}";

            IAIQuestionService aiService = provider == "gemini"
                ? _geminiService
                : _anthropicService;

            try
            {
                var review = await aiService.ReviewCodeSubmissionAsync(
                    topic, taskDescription, submission.Content, assignment.MaxScore);

                return Json(new
                {
                    success = true,
                    suggestedScore = review.SuggestedScore,
                    summary = review.Summary,
                    strengths = review.Strengths,
                    improvements = review.Improvements,
                    missingParts = review.MissingParts
                });
            }
            catch (AIServiceException aiEx)
            {
                return Json(new { success = false, error = aiEx.UserFriendlyMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"AI değerlendirme başarısız: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkAIReviewSubmissions(int classGroupId, string provider = "anthropic")
        {
            var allSubmissions = await _submissionService.GetByClassGroupIdAsync(classGroupId);
            var ungradedCodeSubmissions = allSubmissions
                .Where(s => s.Assignment.Type == "CodeTask" && s.Score == null)
                .ToList();

            if (!ungradedCodeSubmissions.Any())
                return Json(new { success = true, message = "Puanlanmamış kod teslimi bulunamadı.", results = Array.Empty<object>() });

            IAIQuestionService aiService = provider == "gemini"
                ? _geminiService
                : _anthropicService;

            var results = new List<object>();
            int successCount = 0;
            int failCount = 0;

            foreach (var submission in ungradedCodeSubmissions)
            {
                try
                {
                    var week = await _context.SyllabusWeeks
                        .Include(w => w.Syllabus).ThenInclude(s => s.Course)
                        .FirstOrDefaultAsync(w => w.Id == submission.Assignment.SyllabusWeekId);

                    var topic = week?.Topic ?? "Bilinmiyor";
                    var taskDescription = $"{submission.Assignment.Description ?? submission.Assignment.Title}\nBeklenen Davranış: {submission.Assignment.ExpectedBehavior ?? "Belirtilmemiş"}";

                    var review = await aiService.ReviewCodeSubmissionAsync(
                        topic, taskDescription, submission.Content, submission.Assignment.MaxScore);

                    var instructorNote = $"🤖 AI Değerlendirmesi:\n{review.Summary}";
                    if (!string.IsNullOrWhiteSpace(review.MissingParts))
                    {
                        instructorNote += $"\n\nEksikler: {review.MissingParts}";
                    }

                    await _submissionService.GradeAsync(submission.Id, review.SuggestedScore, instructorNote);

                    results.Add(new
                    {
                        studentName = submission.User.FullName,
                        assignmentTitle = submission.Assignment.Title,
                        score = review.SuggestedScore,
                        maxScore = submission.Assignment.MaxScore,
                        success = true,
                        error = (string?)null
                    });
                    successCount++;
                }
                catch (AIServiceException aiEx)
                {
                    results.Add(new
                    {
                        studentName = submission.User.FullName,
                        assignmentTitle = submission.Assignment.Title,
                        score = 0,
                        maxScore = submission.Assignment.MaxScore,
                        success = false,
                        error = aiEx.UserFriendlyMessage
                    });
                    failCount++;
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        studentName = submission.User.FullName,
                        assignmentTitle = submission.Assignment.Title,
                        score = 0,
                        maxScore = submission.Assignment.MaxScore,
                        success = false,
                        error = ex.Message
                    });
                    failCount++;
                }
            }

            return Json(new
            {
                success = true,
                totalCount = ungradedCodeSubmissions.Count,
                successCount,
                failCount,
                results
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission(GradeSubmissionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _submissionService.GradeAsync(model.SubmissionId, model.Score, model.InstructorNote);
            TempData["Success"] = "Puan başarıyla kaydedildi.";
            return RedirectToAction(nameof(Detail), new { id = model.ClassGroupId });
        }
    }
}
