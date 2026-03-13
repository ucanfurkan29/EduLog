using EduLog.Core.Entities;
using EduLog.Data;
using EduLog.Services;
using EduLog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class SubmissionsController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ISubmissionService _submissionService;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubmissionsController(
            IAssignmentService assignmentService,
            ISubmissionService submissionService,
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> SubmitCode(int assignmentId, int classGroupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var assignment = await _assignmentService.GetByIdAsync(assignmentId);
            if (assignment == null)
            {
                TempData["Error"] = "Ödev bulunamadı.";
                return RedirectToAction("Index", "StudentDashboard");
            }

            // Check if already submitted
            var existing = await _submissionService.GetByAssignmentAndUserAsync(assignmentId, user.Id);
            if (existing != null)
            {
                return View("CodeResult", new CodeSubmissionResultViewModel
                {
                    SubmissionId = existing.Id,
                    AssignmentTitle = assignment.Title,
                    MaxScore = assignment.MaxScore,
                    Score = existing.Score,
                    InstructorNote = existing.InstructorNote,
                    SubmittedAt = existing.SubmittedAt,
                    ClassGroupId = classGroupId,
                    SubmittedContent = existing.Content
                });
            }

            var model = new SubmitCodeViewModel
            {
                AssignmentId = assignmentId,
                ClassGroupId = classGroupId,
                AssignmentTitle = assignment.Title,
                AssignmentDescription = assignment.Description,
                ExpectedBehavior = assignment.ExpectedBehavior,
                StarterCode = assignment.StarterCode,
                MaxScore = assignment.MaxScore,
                Content = assignment.StarterCode ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitCode(SubmitCodeViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                var assignment = await _assignmentService.GetByIdAsync(model.AssignmentId);
                model.AssignmentTitle = assignment?.Title ?? "";
                model.AssignmentDescription = assignment?.Description;
                model.ExpectedBehavior = assignment?.ExpectedBehavior;
                model.StarterCode = assignment?.StarterCode;
                model.MaxScore = assignment?.MaxScore ?? 0;
                return View(model);
            }

            // Check for duplicate
            var existing = await _submissionService.GetByAssignmentAndUserAsync(model.AssignmentId, user.Id);
            if (existing != null)
            {
                TempData["Error"] = "Bu ödevi zaten teslim ettiniz.";
                return RedirectToAction("Index", "StudentDashboard");
            }

            var submission = new Submission
            {
                AssignmentId = model.AssignmentId,
                ClassGroupId = model.ClassGroupId,
                UserId = user.Id,
                Content = model.Content
            };

            await _submissionService.SubmitAsync(submission);

            var assignmentForResult = await _assignmentService.GetByIdAsync(model.AssignmentId);

            return View("CodeResult", new CodeSubmissionResultViewModel
            {
                SubmissionId = submission.Id,
                AssignmentTitle = assignmentForResult?.Title ?? "",
                MaxScore = assignmentForResult?.MaxScore ?? 0,
                Score = null,
                SubmittedAt = submission.SubmittedAt,
                ClassGroupId = model.ClassGroupId,
                SubmittedContent = model.Content
            });
        }

        [HttpGet]
        public async Task<IActionResult> SubmitQuiz(int assignmentId, int classGroupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var assignment = await _assignmentService.GetByIdWithQuestionsAsync(assignmentId);
            if (assignment == null)
            {
                TempData["Error"] = "Ödev bulunamadı.";
                return RedirectToAction("Index", "StudentDashboard");
            }

            // Check if already submitted
            var existing = await _submissionService.GetByAssignmentAndUserAsync(assignmentId, user.Id);
            if (existing != null)
            {
                // Parse stored answers to calculate correct count
                var correctCount = CalculateCorrectCount(existing.Content, assignment.Questions);

                return View("QuizResult", new QuizResultViewModel
                {
                    SubmissionId = existing.Id,
                    AssignmentTitle = assignment.Title,
                    Score = existing.Score ?? 0,
                    MaxScore = assignment.MaxScore,
                    CorrectCount = correctCount,
                    TotalCount = assignment.Questions.Count,
                    ClassGroupId = classGroupId,
                    QuestionResults = BuildQuestionResults(existing.Content, assignment.Questions)
                });
            }

            var model = new SubmitQuizViewModel
            {
                AssignmentId = assignmentId,
                ClassGroupId = classGroupId,
                AssignmentTitle = assignment.Title,
                MaxScore = assignment.MaxScore,
                Questions = assignment.Questions.OrderBy(q => q.OrderIndex).Select(q => new QuizQuestionViewModel
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    OrderIndex = q.OrderIndex
                }).ToList(),
                Answers = assignment.Questions.Select(_ => "").ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(SubmitQuizViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var assignment = await _assignmentService.GetByIdWithQuestionsAsync(model.AssignmentId);
            if (assignment == null)
            {
                TempData["Error"] = "Ödev bulunamadı.";
                return RedirectToAction("Index", "StudentDashboard");
            }

            // Check for duplicate
            var existing = await _submissionService.GetByAssignmentAndUserAsync(model.AssignmentId, user.Id);
            if (existing != null)
            {
                TempData["Error"] = "Bu quiz'i zaten teslim ettiniz.";
                return RedirectToAction("Index", "StudentDashboard");
            }

            // Build answers string
            var answersString = string.Join(",", model.Answers ?? new List<string>());

            // Auto-grade
            var score = _submissionService.AutoGradeQuiz(answersString, assignment.Questions, assignment.MaxScore);
            var correctCount = CalculateCorrectCount(answersString, assignment.Questions);

            var submission = new Submission
            {
                AssignmentId = model.AssignmentId,
                ClassGroupId = model.ClassGroupId,
                UserId = user.Id,
                Content = answersString,
                Score = score
            };

            await _submissionService.SubmitAsync(submission);

            return View("QuizResult", new QuizResultViewModel
            {
                SubmissionId = submission.Id,
                AssignmentTitle = assignment.Title,
                Score = score,
                MaxScore = assignment.MaxScore,
                CorrectCount = correctCount,
                TotalCount = assignment.Questions.Count,
                ClassGroupId = model.ClassGroupId,
                QuestionResults = BuildQuestionResults(answersString, assignment.Questions)
            });
        }

        private List<QuizQuestionResultItem> BuildQuestionResults(string answersString, ICollection<AssignmentQuestion> questions)
        {
            var answers = answersString.Split(',');
            var orderedQuestions = questions.OrderBy(q => q.OrderIndex).ToList();
            var results = new List<QuizQuestionResultItem>();
            for (int i = 0; i < orderedQuestions.Count; i++)
            {
                var q = orderedQuestions[i];
                var studentAnswer = i < answers.Length ? answers[i].Trim() : "";
                results.Add(new QuizQuestionResultItem
                {
                    QuestionText = q.QuestionText,
                    StudentAnswer = studentAnswer,
                    CorrectAnswer = q.CorrectAnswer,
                    IsCorrect = string.Equals(studentAnswer, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase),
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD
                });
            }
            return results;
        }

        private int CalculateCorrectCount(string answersString, ICollection<AssignmentQuestion> questions)
        {
            var answers = answersString.Split(',');
            var orderedQuestions = questions.OrderBy(q => q.OrderIndex).ToList();
            int correct = 0;
            for (int i = 0; i < orderedQuestions.Count && i < answers.Length; i++)
            {
                if (string.Equals(answers[i].Trim(), orderedQuestions[i].CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                    correct++;
            }
            return correct;
        }
    }
}
