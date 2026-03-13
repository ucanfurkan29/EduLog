using EduLog.Core.Entities;
using EduLog.Services;
using EduLog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduLog.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class SyllabiController : Controller
    {
        private readonly ISyllabusService _syllabusService;
        private readonly ICourseService _courseService;

        public SyllabiController(ISyllabusService syllabusService, ICourseService courseService)
        {
            _syllabusService = syllabusService;
            _courseService = courseService;
        }

        public async Task<IActionResult> Index(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                TempData["Error"] = "Ders bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            ViewBag.Course = course;
            var syllabi = await _syllabusService.GetByCourseIdAsync(courseId);
            return View(syllabi);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                TempData["Error"] = "Ders bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            var model = new CreateSyllabusViewModel
            {
                CourseId = courseId,
                CourseName = course.Name
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSyllabusViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var course = await _courseService.GetCourseByIdAsync(model.CourseId);
                model.CourseName = course?.Name ?? "";
                return View(model);
            }

            var syllabus = new Syllabus
            {
                CourseId = model.CourseId,
                Title = model.Title,
                CreatedAt = DateTime.UtcNow
            };

            await _syllabusService.CreateAsync(syllabus);
            TempData["Success"] = $"'{syllabus.Title}' müfredatı oluşturuldu.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var syllabus = await _syllabusService.GetByIdAsync(id);
            if (syllabus == null)
            {
                TempData["Error"] = "Müfredat bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            var model = new EditSyllabusViewModel
            {
                Id = syllabus.Id,
                CourseId = syllabus.CourseId,
                CourseName = syllabus.Course.Name,
                Title = syllabus.Title
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditSyllabusViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var course = await _courseService.GetCourseByIdAsync(model.CourseId);
                model.CourseName = course?.Name ?? "";
                return View(model);
            }

            var syllabus = await _syllabusService.GetByIdAsync(model.Id);
            if (syllabus == null)
            {
                TempData["Error"] = "Müfredat bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            syllabus.Title = model.Title;
            await _syllabusService.UpdateAsync(syllabus);

            TempData["Success"] = $"'{syllabus.Title}' müfredatı güncellendi.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var syllabus = await _syllabusService.GetByIdAsync(id);
            if (syllabus == null)
            {
                TempData["Error"] = "Müfredat bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            var courseId = syllabus.CourseId;
            await _syllabusService.DeleteAsync(id);
            TempData["Success"] = $"'{syllabus.Title}' müfredatı silindi.";
            return RedirectToAction(nameof(Index), new { courseId });
        }
    }
}
