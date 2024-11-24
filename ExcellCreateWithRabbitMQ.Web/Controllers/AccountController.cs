using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExcellCreateWithRabbitMQ.Web.Controllers
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

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var hasUser = await _userManager.FindByEmailAsync(email);
            if(hasUser is null)
            {
                ViewBag.Message = "Hatalı Kullanıcı Adı veya Şifre";
                return View();
            }

            var signInResult = await _signInManager.PasswordSignInAsync(hasUser, password, true,false);
            if(!signInResult.Succeeded)
            {
                ViewBag.Message = "Hatalı Kullanıcı Adı veya Şifre";
                return View();
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
