using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.LoginScripts;
using T1_Wormhole_2._0._1.Models.DTOs;


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
                TempData["Email"] = user.Email;
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

        //重送Email
        [HttpGet]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            email = TempData["Email"]?.ToString(); //用TempData把Email帶過來
            TempData.Keep("Email");
            var user = _userService.FindByEmail(email);

            if (user != null && !user.EmailConfirmed)
            {
                user.EmailVerificationToken = Guid.NewGuid().ToString("N");
                _userService.UpdateUser(user);
                var baseUrl = _configuration["AppSettings:BaseUrl"];
                var verificationLink = $"{baseUrl}/Account/VerifyEmail?token={user.EmailVerificationToken}&email={email}";

                var emailMessage = $@"
                <h2>Welcome to Wormhole</h2>
                <p>Please verify your email by clicking the link below:</p>
                <a href='{verificationLink}'>Verify Email</a>";

                await _emailSender.SendEmailAsync(email,
                    "Verify Your Email", emailMessage);

                return RedirectToAction("RegisterConfirmation", "Account");
            }
            return View("Error");
        }
        // 重送email部分結束

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


                    var authProperties = new AuthenticationProperties 
                    {
                        IsPersistent = model.KeepLog == null ? false : true //保持登入
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
            Console.WriteLine($"Login attempt for email: {model.UserIdentifier} fail");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO model)
        {
            //sent letter only
            var user = _userService.FindByEmail(model.Email);
            if (ModelState.IsValid && user != null)
            {
                if (user.EmailConfirmed)
                {
                    user.EmailVerificationToken = Guid.NewGuid().ToString("N");
                    _userService.UpdateUser(user);
                    var baseUrl = _configuration["AppSettings:BaseUrl"];
                    var verificationLink = $"{baseUrl}/Account/ResetPassword?token={user.EmailVerificationToken}&email={user.Email}";

                    var emailMessage = $@"
                    <h2>Welcome to Wormhole</h2>
                    <p>Please reset password by clicking the link below:</p>
                    <a href='{verificationLink}'>Reset Password</a>";

                    await _emailSender.SendEmailAsync(user.Email,
                        "Reset Your Password", emailMessage);

                    TempData["Email"] = user.Email;
                    TempData["token"] = user.EmailVerificationToken;
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                return View(model);
            }
            Console.WriteLine("something wrong with input data");
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            Console.WriteLine($"VerifyEmail called with token: {token}, email: {email}");

            // Check if token or email is null or empty
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ViewBag.ErrorMessage = "Invalid token or email.";
                return View("Error");
            }
            TempData["Email"] = email;
            TempData["token"] = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            model.Email = TempData["Email"]?.ToString();
            model.EmailVerificationToken = TempData["token"]?.ToString();
            var user = _userService.FindByEmail(model.Email);
            TempData.Keep("Email");
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                Console.WriteLine("pwd incorrect");
                return View(model);
            }
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                Console.WriteLine("can't find user");
                return View(model);
            }

            if (user.EmailVerificationToken != model.EmailVerificationToken)
            {
                ModelState.AddModelError("", "Invalid token.");
                Console.WriteLine("token mismatch");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine($"{user.Account}");
                user.EmailVerificationToken = null;
                user.Password = _passwordHasher.HashPassword(model.Password);
                _userService.UpdateUser(user);
                // Update the user
                Console.WriteLine("Reseting user password");

                return RedirectToAction("Index", "Home");
            }
            Console.WriteLine("something wrong");
            return View(model);
        }
    }
}

