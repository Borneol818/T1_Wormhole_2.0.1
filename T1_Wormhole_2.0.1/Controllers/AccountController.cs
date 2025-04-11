using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.LoginScripts;


namespace T1_Wormhole_2._0._1.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AccountController(IUserService userService, IPasswordHasher passwordHasher, IEmailSender emailSender, IConfiguration configuration)
        {
            _userService = userService;
            _passwordHasher = passwordHasher;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Login model)
        {
            var ExistUser = _userService.FindByUsername(model.Account);
            if (model.Password != model.ConfirmPassword || (ExistUser != null))
            {
                ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");
                Console.WriteLine("帳號創建失敗");
                return View(model);
            }
            
            if (ModelState.IsValid)
            {
                var user = new Login
                {
                    Account = model.Account,
                    Email = model.Email,
                    EmailConfirmed = false,
                    EmailVerificationToken = Guid.NewGuid().ToString("N")
                };
                user.Password = _passwordHasher.HashPassword(model.Password);

                _userService.CreateUser(user);
                var baseUrl = _configuration["AppSettings:BaseUrl"];
                var verificationLink = $"{baseUrl}/Account/VerifyEmail?token={user.EmailVerificationToken}&email={user.Email}";

                var emailMessage = $@"
                <h2>Welcome to Wormhole</h2>
                <p>Please verify your email by clicking the link below:</p>
                <a href='{verificationLink}'>Verify Email</a>";

                await _emailSender.SendEmailAsync(user.Email,
                    "Verify Your Email",emailMessage);

                return RedirectToAction("RegisterConfirmation");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterConfirmation() 
        {
            return View(); 
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token, string email)
        {
            Console.WriteLine($"VerifyEmail called with token: {token}, email: {email}");

            // Check if token or email is null or empty
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                Console.WriteLine("Token or email is null or empty");
                return View("Error");
            }
            var user = _userService.FindByEmail(email);
            if (user == null)
            {
                Console.WriteLine($"User not found for email: {email}");
                return View("Error");
            }

            // Check if the token matches
            if (user.EmailVerificationToken != token)
            {
                Console.WriteLine($"Token mismatch. Expected: {user.EmailVerificationToken}, Received: {token}");
                return View("Error");
            }

            // Update the user
            Console.WriteLine("Updating user email confirmation status");
            user.EmailConfirmed = true;
            user.EmailVerificationToken = null;

            try
            {
                _userService.UpdateUser(user);
                Console.WriteLine("User updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return View("Error");
            }
            Console.WriteLine("Redirecting to Login");
            return RedirectToAction("Login");
        }
        

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            
            if (ModelState.IsValid)
            {
                
                var user = _userService.FindByEmail(model.UserIdentifier);

                // If not found by email, try username
                if (user == null)
                {
                    user = _userService.FindByUsername(model.UserIdentifier);
                }
                Console.WriteLine($"User found: {user != null}");
                if (user != null && _passwordHasher.VerifyPassword(user.Password, model.Password) && user.EmailConfirmed)
                {
   
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Account),
                        new Claim(ClaimTypes.Role, "User"),
                    };

                    //做一張身分證叫做cookies
                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);


                    //放身分證在原則內
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);


                    var authProperties = new AuthenticationProperties { };

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
            Console.WriteLine($"Login attempt for email: {model.UserIdentifier} fail");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
