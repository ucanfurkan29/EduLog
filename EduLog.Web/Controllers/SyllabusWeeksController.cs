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
    public class SyllabusWeeksController : Controller
    {
        private readonly ISyllabusService _syllabusService;
        private readonly IWeekResourceService _weekResourceService;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedExtensions = { ".pdf", ".zip", ".cs", ".py", ".txt", ".md" };
        private const long MaxFileSize = 20 * 1024 * 1024; // 20 MB

        public SyllabusWeeksController(
            ISyllabusService syllabusService,
            IWeekResourceService weekResourceService,
            AppDbContext context,
            IWebHostEnvironment env)
        {
            _syllabusService = syllabusService;
            _weekResourceService = weekResourceService;
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int syllabusId)
        {
            var syllabus = await _syllabusService.GetByIdWithWeeksAsync(syllabusId);
            if (syllabus == null)
            {
                TempData["Error"] = "Müfredat bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            ViewBag.Syllabus = syllabus;
            ViewBag.CourseName = syllabus.Course.Name;

            var weeks = syllabus.Weeks.OrderBy(w => w.WeekNumber).ToList();
            return View(weeks);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int syllabusId)
        {
            var syllabus = await _syllabusService.GetByIdAsync(syllabusId);
            if (syllabus == null)
            {
                TempData["Error"] = "Müfredat bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            // Sonraki hafta numarasını otomatik belirle
            var maxWeek = await _context.SyllabusWeeks
                .Where(w => w.SyllabusId == syllabusId)
                .MaxAsync(w => (int?)w.WeekNumber) ?? 0;

            var model = new CreateSyllabusWeekViewModel
            {
                SyllabusId = syllabusId,
                SyllabusTitle = syllabus.Title,
                CourseName = syllabus.Course.Name,
                WeekNumber = maxWeek + 1
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSyllabusWeekViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var syllabus = await _syllabusService.GetByIdAsync(model.SyllabusId);
                model.SyllabusTitle = syllabus?.Title ?? "";
                model.CourseName = syllabus?.Course?.Name ?? "";
                return View(model);
            }

            var week = new SyllabusWeek
            {
                SyllabusId = model.SyllabusId,
                WeekNumber = model.WeekNumber,
                Topic = model.Topic,
                Examples = model.Examples,
                Notes = model.Notes
            };

            _context.SyllabusWeeks.Add(week);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Hafta {week.WeekNumber} eklendi.";
            return RedirectToAction(nameof(Index), new { syllabusId = model.SyllabusId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var week = await _context.SyllabusWeeks
                .Include(w => w.Syllabus)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (week == null)
            {
                TempData["Error"] = "Hafta bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            var model = new EditSyllabusWeekViewModel
            {
                Id = week.Id,
                SyllabusId = week.SyllabusId,
                SyllabusTitle = week.Syllabus.Title,
                CourseName = week.Syllabus.Course.Name,
                WeekNumber = week.WeekNumber,
                Topic = week.Topic,
                Examples = week.Examples,
                Notes = week.Notes
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditSyllabusWeekViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var syllabus = await _syllabusService.GetByIdAsync(model.SyllabusId);
                model.SyllabusTitle = syllabus?.Title ?? "";
                model.CourseName = syllabus?.Course?.Name ?? "";
                return View(model);
            }

            var week = await _context.SyllabusWeeks.FindAsync(model.Id);
            if (week == null)
            {
                TempData["Error"] = "Hafta bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            week.WeekNumber = model.WeekNumber;
            week.Topic = model.Topic;
            week.Examples = model.Examples;
            week.Notes = model.Notes;

            _context.SyllabusWeeks.Update(week);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Hafta {week.WeekNumber} güncellendi.";
            return RedirectToAction(nameof(Index), new { syllabusId = model.SyllabusId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var week = await _context.SyllabusWeeks
                .Include(w => w.Resources)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (week == null)
            {
                TempData["Error"] = "Hafta bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            var syllabusId = week.SyllabusId;

            // Dosyaları fiziksel olarak sil
            foreach (var resource in week.Resources)
            {
                var fullPath = Path.Combine(_env.WebRootPath, resource.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            _context.SyllabusWeeks.Remove(week);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Hafta {week.WeekNumber} silindi.";
            return RedirectToAction(nameof(Index), new { syllabusId });
        }

        // Hafta detay sayfası (dosya yükleme ve listeleme)
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var week = await _context.SyllabusWeeks
                .Include(w => w.Syllabus)
                    .ThenInclude(s => s.Course)
                .Include(w => w.Resources)
                .Include(w => w.Assignments)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (week == null)
            {
                TempData["Error"] = "Hafta bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            var model = new SyllabusWeekDetailViewModel
            {
                Week = week,
                SyllabusTitle = week.Syllabus.Title,
                CourseName = week.Syllabus.Course.Name,
                SyllabusId = week.SyllabusId,
                Resources = week.Resources.OrderBy(r => r.FileName),
                Assignments = week.Assignments.OrderBy(a => a.Id)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(FileUploadViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                TempData["Error"] = "Lütfen bir dosya seçin.";
                return RedirectToAction(nameof(Detail), new { id = model.SyllabusWeekId });
            }

            if (model.File.Length > MaxFileSize)
            {
                TempData["Error"] = "Dosya boyutu 20 MB'ı aşamaz.";
                return RedirectToAction(nameof(Detail), new { id = model.SyllabusWeekId });
            }

            var ext = Path.GetExtension(model.File.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                TempData["Error"] = $"Bu dosya uzantısına izin verilmiyor. İzin verilen: {string.Join(", ", AllowedExtensions)}";
                return RedirectToAction(nameof(Detail), new { id = model.SyllabusWeekId });
            }

            var week = await _context.SyllabusWeeks.FindAsync(model.SyllabusWeekId);
            if (week == null)
            {
                TempData["Error"] = "Hafta bulunamadı.";
                return RedirectToAction("Index", "Courses");
            }

            // Dosya yolu: wwwroot/uploads/{syllabusId}/{weekNumber}/
            var relativePath = Path.Combine("uploads", model.SyllabusId.ToString(), week.WeekNumber.ToString());
            var uploadDir = Path.Combine(_env.WebRootPath, relativePath);

            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            // Benzersiz dosya adı
            var fileName = $"{Guid.NewGuid():N}_{model.File.FileName}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            var resource = new WeekResource
            {
                SyllabusWeekId = model.SyllabusWeekId,
                FileName = model.File.FileName,
                FilePath = $"/{relativePath}/{fileName}".Replace("\\", "/"),
                ResourceType = ext.TrimStart('.')
            };

            await _weekResourceService.CreateAsync(resource);

            TempData["Success"] = $"'{model.File.FileName}' dosyası yüklendi.";
            return RedirectToAction(nameof(Detail), new { id = model.SyllabusWeekId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int id, int weekId)
        {
            var resource = await _weekResourceService.GetByIdAsync(id);
            if (resource == null)
            {
                TempData["Error"] = "Dosya bulunamadı.";
                return RedirectToAction(nameof(Detail), new { id = weekId });
            }

            // Fiziksel dosyayı sil
            var fullPath = Path.Combine(_env.WebRootPath, resource.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            await _weekResourceService.DeleteAsync(id);

            TempData["Success"] = $"'{resource.FileName}' dosyası silindi.";
            return RedirectToAction(nameof(Detail), new { id = weekId });
        }
    }
}
