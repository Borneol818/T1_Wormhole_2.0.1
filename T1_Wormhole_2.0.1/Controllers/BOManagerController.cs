using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using T1_Wormhole_2._0._1.LoginScripts;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;


namespace T1_Wormhole_2._0._1.Controllers
{
    public class BOManagerController : Controller
    {
        private readonly WormHoleContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public BOManagerController(WormHoleContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher();
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login() 
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(BOManagerLoginDTO model)
        {
            if (ModelState.IsValid)
            {

                var admin = _context.BoManagers.FirstOrDefault(m => m.Account == model.Account);

                Console.WriteLine($"Admin found: {admin != null}");
                var AdminId = Convert.ToString(admin.ManagerId);
                if (admin != null && _passwordHasher.VerifyPassword(admin.Password, model.Password))
                {

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, admin.Account),
                        new Claim(ClaimTypes.NameIdentifier, AdminId),
                        new Claim(ClaimTypes.Role, "Admin"),
                    };

                    //做一張身分證叫做cookies
                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);


                    //放身分證在原則內
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);


                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false
                    };



                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        claimsPrincipal,
                        authProperties);

                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    Console.WriteLine("帳號或密碼錯誤");
                    return View(model);
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            Console.WriteLine($"Login attempt for email: {model.Account} fail");
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Logout() 
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index","Home");
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
