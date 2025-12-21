using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using System.Threading.Tasks;

namespace Restaurant.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // get /Account/Auth
        [HttpGet]
        public IActionResult Auth()
        {
            var model = new AuthViewModel();
            return View(model);
        }

        // post /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(AuthViewModel modelWrapper)
        {
            var model = modelWrapper.Login;

            // были странные ошибк поэтому просто убрал их
            ModelState.Remove("Register.Email");
            ModelState.Remove("Register.Password");
            ModelState.Remove("Register.ConfirmPassword");

            // проверка валидации
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Catalog", "Items");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            // if провал - возвращаем на вкладку Login
            ViewData["ActiveTab"] = "login";
            return View("Auth", modelWrapper);
        }

        // post /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(AuthViewModel modelWrapper)
        {
            var model = modelWrapper.Register;

            // опять ошибки
            ModelState.Remove("Login.Email");
            ModelState.Remove("Login.Password");

            //  проверяем валидацию
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Catalog", "Items");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Если провал - возвращаем на вкладку Register
            ViewData["ActiveTab"] = "register";
            return View("Auth", modelWrapper);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Catalog", "Items");
        }
    }
}