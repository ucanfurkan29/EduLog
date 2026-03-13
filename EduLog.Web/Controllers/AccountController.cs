using EduLog.Core.Entities;
using EduLog.Data;
using EduLog.Services;
using EduLog.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLog.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IClassGroupService _classGroupService;
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IClassGroupService classGroupService,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _classGroupService = classGroupService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Instructor"))
                        return RedirectToAction("Index", "InstructorDashboard");
                    else
                        return RedirectToAction("Index", "StudentDashboard");
                }
                return RedirectToLocal(returnUrl);
            }

            TempData["Error"] = "Geçersiz e-posta veya şifre.";
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. JoinCode ile eşleşen ClassGroup bul
            var classGroup = await _classGroupService.GetByJoinCodeAsync(model.JoinCode.ToUpper());
            if (classGroup == null)
            {
                TempData["Error"] = "Geçersiz sınıf kodu. Lütfen doğru bir katılım kodu giriniz.";
                return View(model);
            }

            // 2. Kullanıcıyı oluştur
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // 3. Student rolü ata
            await _userManager.AddToRoleAsync(user, "Student");

            // 4. ClassEnrollment kaydı oluştur
            var enrollment = new ClassEnrollment
            {
                ClassGroupId = classGroup.Id,
                UserId = user.Id,
                JoinedAt = DateTime.UtcNow
            };
            _context.ClassEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Otomatik giriş yap
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "StudentDashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
