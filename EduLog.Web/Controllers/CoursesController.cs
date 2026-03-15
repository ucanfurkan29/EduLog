using EduLog.Core.Entities;
using EduLog.Services;
using EduLog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduLog.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class CoursesController : Controller
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return View(courses);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCourseViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var course = new Course
            {
                Name = model.Name,
                Description = model.Description,
                ProgrammingLanguage = model.ProgrammingLanguage
            };

            await _courseService.CreateAsync(course);
            TempData["Success"] = $"'{course.Name}' dersi başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                TempData["Error"] = "Ders bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditCourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                ProgrammingLanguage = course.ProgrammingLanguage
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCourseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var course = await _courseService.GetCourseByIdAsync(model.Id);
            if (course == null)
            {
                TempData["Error"] = "Ders bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            course.Name = model.Name;
            course.Description = model.Description;
            course.ProgrammingLanguage = model.ProgrammingLanguage;
            await _courseService.UpdateAsync(course);

            TempData["Success"] = $"'{course.Name}' dersi güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                TempData["Error"] = "Ders bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            await _courseService.DeleteAsync(id);
            TempData["Success"] = $"'{course.Name}' dersi silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
