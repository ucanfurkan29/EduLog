using EduLog.Core.Entities;
using EduLog.Data;
using EduLog.Services;
using EduLog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class AssignmentsController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IAnthropicService _anthropicService;
        private readonly GeminiService _geminiService;
        private readonly AppDbContext _context;

        public AssignmentsController(
            IAssignmentService assignmentService,
            IAnthropicService anthropicService,
            GeminiService geminiService,
            AppDbContext context)
        {
            _assignmentService = assignmentService;
            _anthropicService = anthropicService;
            _geminiService = geminiService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int syllabusWeekId)
        {
            var week = await _context.SyllabusWeeks
                .Include(w => w.Syllabus)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(w => w.Id == syllabusWeekId);

            if (week == null)
            {
                TempData["Error"] = "Ders bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            var model = new CreateAssignmentViewModel
            {
                SyllabusWeekId = syllabusWeekId,
                WeekTopic = week.Topic,
                CourseName = week.Syllabus.Course.Name,
                WeekNumber = week.WeekNumber,
                SyllabusId = week.SyllabusId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAssignmentViewModel model)
        {
            // Remove question validations for CodeTask type
            if (model.Type == "CodeTask")
            {
                model.Questions.Clear();
                // Remove question-related model state errors
                foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Questions")).ToList())
                    ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                var week = await _context.SyllabusWeeks
                    .Include(w => w.Syllabus).ThenInclude(s => s.Course)
                    .FirstOrDefaultAsync(w => w.Id == model.SyllabusWeekId);
                model.WeekTopic = week?.Topic ?? "";
                model.CourseName = week?.Syllabus?.Course?.Name ?? "";
                model.WeekNumber = week?.WeekNumber ?? 0;
                model.SyllabusId = week?.SyllabusId ?? 0;
                return View(model);
            }

            var assignment = new Assignment
            {
                SyllabusWeekId = model.SyllabusWeekId,
                Title = model.Title,
                Description = model.Description,
                Type = model.Type,
                MaxScore = model.MaxScore,
                DueDate = model.DueDate,
                IsAIGenerated = false
            };

            if (model.Type == "MultipleChoice")
            {
                for (int i = 0; i < model.Questions.Count; i++)
                {
                    assignment.Questions.Add(new AssignmentQuestion
                    {
                        QuestionText = model.Questions[i].QuestionText,
                        OptionA = model.Questions[i].OptionA,
                        OptionB = model.Questions[i].OptionB,
                        OptionC = model.Questions[i].OptionC,
                        OptionD = model.Questions[i].OptionD,
                        CorrectAnswer = model.Questions[i].CorrectAnswer,
                        OrderIndex = i
                    });
                }
            }

            await _assignmentService.CreateAsync(assignment);
            TempData["Success"] = $"'{assignment.Title}' ödevi oluşturuldu.";
            return RedirectToAction("Detail", "SyllabusWeeks", new { id = model.SyllabusWeekId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAI(int syllabusWeekId, string provider = "anthropic", string type = "multipleChoice", int questionCount = 5)
        {
            var week = await _context.SyllabusWeeks
                .Include(w => w.Syllabus).ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(w => w.Id == syllabusWeekId);

            if (week == null)
            {
                TempData["Error"] = "Ders bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            IAIQuestionService aiService = provider == "gemini"
                ? _geminiService
                : _anthropicService;

            var providerName = provider == "gemini" ? "Gemini (Google)" : "Claude (Anthropic)";

            try
            {
                if (type == "codeTask")
                {
                    var codeTask = await aiService.GenerateCodeTaskAsync(
                        week.Topic, week.Notes, week.Examples);

                    if (string.IsNullOrWhiteSpace(codeTask.Title) && string.IsNullOrWhiteSpace(codeTask.Description))
                    {
                        TempData["Error"] = "AI kod ödevi üretemedi. Lütfen tekrar deneyin.";
                        return RedirectToAction("Detail", "SyllabusWeeks", new { id = syllabusWeekId });
                    }

                    var model = new AICodeTaskPreviewViewModel
                    {
                        SyllabusWeekId = syllabusWeekId,
                        WeekTopic = week.Topic,
                        CourseName = week.Syllabus.Course.Name,
                        WeekNumber = week.WeekNumber,
                        SyllabusId = week.SyllabusId,
                        Provider = providerName,
                        Title = codeTask.Title,
                        Description = codeTask.Description,
                        ExpectedBehavior = codeTask.ExpectedBehavior,
                        StarterCode = codeTask.StarterCode,
                        MaxScore = 100
                    };

                    return View("PreviewAICodeTask", model);
                }
                else
                {
                    // MultipleChoice — existing flow
                    var aiQuestions = await aiService.GenerateQuestionsAsync(
                        week.Topic, week.Notes, week.Examples, questionCount);

                    if (!aiQuestions.Any())
                    {
                        TempData["Error"] = "AI soru üretemedi. Lütfen tekrar deneyin.";
                        return RedirectToAction("Detail", "SyllabusWeeks", new { id = syllabusWeekId });
                    }

                    var model = new AIPreviewViewModel
                    {
                        SyllabusWeekId = syllabusWeekId,
                        WeekTopic = week.Topic,
                        CourseName = week.Syllabus.Course.Name,
                        WeekNumber = week.WeekNumber,
                        SyllabusId = week.SyllabusId,
                        Provider = providerName,
                        Title = $"Ders {week.WeekNumber} - {week.Topic} Quiz",
                        MaxScore = 100,
                        Questions = aiQuestions.Select((q, i) => new QuestionViewModel
                        {
                            QuestionText = q.QuestionText,
                            OptionA = q.OptionA,
                            OptionB = q.OptionB,
                            OptionC = q.OptionC,
                            OptionD = q.OptionD,
                            CorrectAnswer = q.CorrectAnswer.ToUpper(),
                            OrderIndex = i
                        }).ToList()
                    };

                    return View("PreviewAI", model);
                }
            }
            catch (AIServiceException aiEx)
            {
                TempData["Error"] = aiEx.UserFriendlyMessage;
                return RedirectToAction("Detail", "SyllabusWeeks", new { id = syllabusWeekId });
            }
            catch (Exception)
            {
                TempData["Error"] = $"{providerName} servisiyle bağlantı kurulamadı. API anahtarınızı kontrol edin.";
                return RedirectToAction("Detail", "SyllabusWeeks", new { id = syllabusWeekId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAI(AIPreviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var week = await _context.SyllabusWeeks
                    .Include(w => w.Syllabus).ThenInclude(s => s.Course)
                    .FirstOrDefaultAsync(w => w.Id == model.SyllabusWeekId);
                model.WeekTopic = week?.Topic ?? "";
                model.CourseName = week?.Syllabus?.Course?.Name ?? "";
                model.WeekNumber = week?.WeekNumber ?? 0;
                model.SyllabusId = week?.SyllabusId ?? 0;
                return View("PreviewAI", model);
            }

            var assignment = new Assignment
            {
                SyllabusWeekId = model.SyllabusWeekId,
                Title = model.Title,
                Description = model.Description,
                Type = "MultipleChoice",
                MaxScore = model.MaxScore,
                DueDate = model.DueDate,
                IsAIGenerated = true
            };

            for (int i = 0; i < model.Questions.Count; i++)
            {
                assignment.Questions.Add(new AssignmentQuestion
                {
                    QuestionText = model.Questions[i].QuestionText,
                    OptionA = model.Questions[i].OptionA,
                    OptionB = model.Questions[i].OptionB,
                    OptionC = model.Questions[i].OptionC,
                    OptionD = model.Questions[i].OptionD,
                    CorrectAnswer = model.Questions[i].CorrectAnswer,
                    OrderIndex = i
                });
            }

            await _assignmentService.CreateAsync(assignment);
            TempData["Success"] = $"AI ile oluşturulan '{assignment.Title}' ödevi kaydedildi.";
            return RedirectToAction("Detail", "SyllabusWeeks", new { id = model.SyllabusWeekId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAICodeTask(AICodeTaskPreviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var week = await _context.SyllabusWeeks
                    .Include(w => w.Syllabus).ThenInclude(s => s.Course)
                    .FirstOrDefaultAsync(w => w.Id == model.SyllabusWeekId);
                model.WeekTopic = week?.Topic ?? "";
                model.CourseName = week?.Syllabus?.Course?.Name ?? "";
                model.WeekNumber = week?.WeekNumber ?? 0;
                model.SyllabusId = week?.SyllabusId ?? 0;
                return View("PreviewAICodeTask", model);
            }

            var assignment = new Assignment
            {
                SyllabusWeekId = model.SyllabusWeekId,
                Title = model.Title,
                Description = model.Description,
                Type = "CodeTask",
                MaxScore = model.MaxScore,
                DueDate = model.DueDate,
                IsAIGenerated = true,
                ExpectedBehavior = model.ExpectedBehavior,
                StarterCode = model.StarterCode
            };

            await _assignmentService.CreateAsync(assignment);
            TempData["Success"] = $"AI ile oluşturulan '{assignment.Title}' kod ödevi kaydedildi.";
            return RedirectToAction("Detail", "SyllabusWeeks", new { id = model.SyllabusWeekId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var assignment = await _assignmentService.GetByIdWithQuestionsAsync(id);
            if (assignment == null)
            {
                TempData["Error"] = "Ödev bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            var model = new EditAssignmentViewModel
            {
                Id = assignment.Id,
                SyllabusWeekId = assignment.SyllabusWeekId,
                WeekTopic = assignment.Week.Topic,
                CourseName = assignment.Week.Syllabus.Course.Name,
                WeekNumber = assignment.Week.WeekNumber,
                SyllabusId = assignment.Week.SyllabusId,
                Title = assignment.Title,
                Description = assignment.Description,
                Type = assignment.Type,
                MaxScore = assignment.MaxScore,
                DueDate = assignment.DueDate,
                Questions = assignment.Questions.OrderBy(q => q.OrderIndex).Select(q => new QuestionViewModel
                {
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    CorrectAnswer = q.CorrectAnswer,
                    OrderIndex = q.OrderIndex
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAssignmentViewModel model)
        {
            if (model.Type == "CodeTask")
            {
                model.Questions.Clear();
                foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Questions")).ToList())
                    ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                var week = await _context.SyllabusWeeks
                    .Include(w => w.Syllabus).ThenInclude(s => s.Course)
                    .FirstOrDefaultAsync(w => w.Id == model.SyllabusWeekId);
                model.WeekTopic = week?.Topic ?? "";
                model.CourseName = week?.Syllabus?.Course?.Name ?? "";
                model.WeekNumber = week?.WeekNumber ?? 0;
                model.SyllabusId = week?.SyllabusId ?? 0;
                return View(model);
            }

            var assignment = await _context.Assignments.FindAsync(model.Id);
            if (assignment == null)
            {
                TempData["Error"] = "Ödev bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            assignment.Title = model.Title;
            assignment.Description = model.Description;
            assignment.Type = model.Type;
            assignment.MaxScore = model.MaxScore;
            assignment.DueDate = model.DueDate;

            var questions = new List<AssignmentQuestion>();
            if (model.Type == "MultipleChoice")
            {
                for (int i = 0; i < model.Questions.Count; i++)
                {
                    questions.Add(new AssignmentQuestion
                    {
                        QuestionText = model.Questions[i].QuestionText,
                        OptionA = model.Questions[i].OptionA,
                        OptionB = model.Questions[i].OptionB,
                        OptionC = model.Questions[i].OptionC,
                        OptionD = model.Questions[i].OptionD,
                        CorrectAnswer = model.Questions[i].CorrectAnswer,
                        OrderIndex = i
                    });
                }
            }

            await _assignmentService.UpdateAsync(assignment, questions);
            TempData["Success"] = $"'{assignment.Title}' ödevi güncellendi.";
            return RedirectToAction("Detail", "SyllabusWeeks", new { id = model.SyllabusWeekId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int syllabusWeekId)
        {
            await _assignmentService.DeleteAsync(id);
            TempData["Success"] = "Ödev silindi.";
            return RedirectToAction("Detail", "SyllabusWeeks", new { id = syllabusWeekId });
        }
    }
}
