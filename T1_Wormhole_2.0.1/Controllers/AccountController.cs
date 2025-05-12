using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.LoginScripts;
using T1_Wormhole_2._0._1.Models.DTOs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using AspNetCoreGeneratedDocument;


namespace T1_Wormhole_2._0._1.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly WormHoleContext _context;

        public AccountController(IDataProtectionProvider dataProtectionProvider, IUserService userService, IPasswordHasher passwordHasher, IEmailSender emailSender, IConfiguration configuration, WormHoleContext context)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _userService = userService;
            _passwordHasher = passwordHasher;
            _emailSender = emailSender;
            _configuration = configuration;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(Login model1, UserInfo model2)
        {
            var ExistUser = _userService.FindByEmail(model1.Email);
            var ExistAccount = _userService.FindByUsername(model1.Account);
            if (model1.Password != model1.ConfirmPassword || (ExistUser != null) || (ExistAccount != null))
            {
                ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");
                Console.WriteLine("帳號創建失敗");
                TempData["RegisterFail"] = "創建帳號失敗!您是否已註冊會員或檢查您的資訊是否有誤";
                return View("Register");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new Login
                    {
                        Account = model1.Account,
                        Email = model1.Email,
                        EmailConfirmed = false,
                        EmailVerificationToken = Guid.NewGuid().ToString("N")
                    };
                    user.Password = _passwordHasher.HashPassword(model1.Password);

                    _userService.CreateUser(user);
                    //新增UserInfo
                    var userInfo = new UserInfo
                    {
                        UserId = 0,
                        Name = model2.Name,
                        Email = model1.Email,
                        Nickname = model2.Nickname,
                        Sex = model2.Sex,
                        Birthday = model2.Birthday,
                        Phone = model2.Phone,
                        Status = false
                    };

                    _context.UserInfos.Add(userInfo);
                    _context.SaveChanges();
                    var TempUserId = _context.UserInfos.FirstOrDefault(u => u.Email == model1.Email).UserId;
                    var userStatus = new UserStatus
                    {
                        Id = TempUserId,
                        Status = false,
                        Level = 1
                    };

                    _context.UserStatuses.Add(userStatus);
                    _context.SaveChanges();
                    var baseUrl = _configuration["AppSettings:BaseUrl"];
                    var verificationLink = $"{baseUrl}/Account/VerifyEmail?token={user.EmailVerificationToken}&email={user.Email}";

                    var emailMessage = $@"
                        <h2>歡迎來到Wormhole</h2>
                        <p>請點擊以下連結驗證您的電子郵件：</p>
                        <a href='{verificationLink}'>驗證Email</a>";

                    await _emailSender.SendEmailAsync(user.Email,
                        "請驗證您的電子郵件", emailMessage);
                    TempData["Email"] = user.Email;
                    return RedirectToAction("RegisterConfirmation");
                }
                return View("Register");
            }
            catch (Exception ex)
            {
                TempData["RegisterFail"] = "創建帳號失敗!您是否已註冊會員或檢查您的資訊是否有誤";
                return View("Register");
            }
        }

        [HttpGet]
        public IActionResult RegisterConfirmation() 
        {
            TempData.Keep("Email");
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
                    <h2>歡迎來到Wormhole</h2>
                    <p>請點擊以下連結驗證您的電子郵件：</p>
                    <a href='{verificationLink}'>驗證Email</a>";

                await _emailSender.SendEmailAsync(email,
                    "請驗證您的電子郵件", emailMessage);

                return RedirectToAction("RegisterConfirmation", "Account");
            }
            return View("Error");
        }
        // 重送email部分結束

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else 
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            try
            {
                ViewBag.BaseUrl = _configuration["AppSettings:BaseUrl"];
                if (ModelState.IsValid)
                {

                    var user = _userService.FindByEmail(model.UserIdentifier);

                    // If not found by email, try username
                    if (user == null)
                    {
                        user = _userService.FindByUsername(model.UserIdentifier);
                    }
                    Console.WriteLine($"User found: {user != null}");
                    string email = user != null ? user.Email : string.Empty;
                    var userInfo = _context.UserInfos.FirstOrDefault(u => u.Email == email);
                    string userId = userInfo.UserId != null ? userInfo.UserId.ToString() : string.Empty;
                    if (user != null && _passwordHasher.VerifyPassword(user.Password, model.Password) && user.EmailConfirmed)
                    {

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Account),
                            new Claim(ClaimTypes.NameIdentifier, userId),
                            new Claim("UserID", userId),
                            new Claim(ClaimTypes.Role, "User"),
                        };

                        //做一張身分證叫做cookies
                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);


                        //放身分證在原則內
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);


                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.KeepLog == null ? false : true, //保持登入
                            ExpiresUtc = DateTime.UtcNow.AddHours(8).AddDays(7),
                            AllowRefresh = true,
                        };

                        if (model.RememberMe != null)
                        {
                            var NameProtector = _dataProtectionProvider.CreateProtector("LoginNameCookie");
                            string encryptedUserIdentifier = NameProtector.Protect(model.UserIdentifier);
                            var PWDProtector = _dataProtectionProvider.CreateProtector("LoginPWDCookie");
                            string encryptedPWD = PWDProtector.Protect(model.Password);
                            CookieOptions options = new CookieOptions
                            {
                                Expires = DateTime.UtcNow.AddDays(30),
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict
                            };
                            HttpContext.Response.Cookies.Append("LoginName", encryptedUserIdentifier, options);
                            HttpContext.Response.Cookies.Append("LoginPassword", encryptedPWD, options);
                        }

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            claimsPrincipal,
                            authProperties);

                        var GetReward = await _userService.DailyRewardAsync(userInfo.UserId); //登入獎勵發放
                        if (GetReward)
                        {
                            TempData["RewardMessage"] = "已獲得每日登入獎勵: 2枚金幣";
                        }
                        else
                        {
                            var LoginRecord = new LoginRecord //新增login紀錄
                            {
                                UserId = userInfo.UserId,
                                Time = DateTime.UtcNow.AddHours(8),
                            };
                            _context.LoginRecords.Add(LoginRecord);
                        }
                        userInfo.Status = true;
                        _context.SaveChanges();
                        HttpContext.Session.SetString($"Visit_{userInfo.UserId}", "Active");

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        Console.WriteLine("帳號或密碼錯誤");
                        TempData["LoginFail"] = "登入失敗!請檢查您的登入資料是否有誤";
                        return View(model);
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt");
                Console.WriteLine($"Login attempt for email: {model.UserIdentifier} fail");
                return View(model);
            }
            catch (Exception ex) 
            {
                TempData["LoginFail"] = "登入失敗!請檢查您的登入資料是否有誤";
                return View(model);
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                RedirectToAction("Index", "Home");
            }
            var userId = Convert.ToInt32(HttpContext.User.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value);
            var user = _context.UserInfos.FirstOrDefault(u => u.UserId == userId);
            user.Status = false;
            _context.SaveChanges();
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
            try
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
                        <h2>歡迎來到Wormhole</h2>
                        <p>請點擊以下連結重設密碼：</p>
                        <a href='{verificationLink}'>重設密碼</a>";

                        await _emailSender.SendEmailAsync(user.Email,
                            "重設您的密碼", emailMessage);

                        TempData["Email"] = user.Email;
                        TempData["token"] = user.EmailVerificationToken;
                        return RedirectToAction("ForgotPasswordConfirmation");
                    }
                    return View(model);
                }
                Console.WriteLine("something wrong with input data");
                TempData["ForgotPasswordFail"] = "電子郵件有誤";
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ForgotPasswordFail"] = "電子郵件有誤";
                return View(model);
            }
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
            try
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
                TempData["ResetPasswordFail"] = "重設密碼失敗!請聯絡管理員";
                return View(model);
            }
            catch (Exception ex) 
            {
                TempData["ResetPasswordFail"] = "重設密碼失敗!請聯絡管理員";
                return View(model);
            }
        }

        public async Task<IActionResult> ValidGoogleLogin()
        {
            string? formCredential = Request.Form["credential"]; //回傳憑證
            string? formToken = Request.Form["g_csrf_token"]; //回傳令牌
            string? cookiesToken = Request.Cookies["g_csrf_token"]; //Cookie 令牌

            // 驗證 Google Token
            GoogleJsonWebSignature.Payload? payload = VerifyGoogleToken(formCredential, formToken, cookiesToken).Result;
            if (payload == null)
            {
                // 驗證失敗
                ViewData["Msg"] = "驗證 Google 授權失敗";
            }
            else
            {
                //驗證成功，取使用者資訊內容
                ViewData["Msg"] = "驗證 Google 授權成功" + "<br>";
                ViewData["Msg"] += "Email:" + payload.Email + "<br>";
                ViewData["Msg"] += "Name:" + payload.Name + "<br>";
                ViewData["Msg"] += "Picture:" + payload.Picture;
            }
            var email = payload.Email;
            TempData["Email"] = email;
            var user = _context.UserInfos.FirstOrDefault(u => u.Email == payload.Email);
            if (user == null)
            {
                return RedirectToAction("SubmitUserInfo", "Account");
            }
            else
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, payload.Name),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, "User"),
                };

                //做一張身分證叫做cookies
                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);


                //放身分證在原則內
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);


                var authProperties = new AuthenticationProperties
                {

                };
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);


                var GetReward = await _userService.DailyRewardAsync(user.UserId); //登入獎勵發放
                if (GetReward)
                {
                    TempData["RewardMessage"] = "已獲得每日登入獎勵: 2枚金幣";
                }
                else { 

                    var LoginRecord = new LoginRecord
                    {
                        UserId = user.UserId,
                        Time = DateTime.UtcNow.AddHours(8),
                    };
                    _context.LoginRecords.Add(LoginRecord);
                }
                user.Status = true;
                _context.SaveChanges();
                HttpContext.Session.SetString($"Visit_{user.UserId}", "Active");
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleToken(string? formCredential, string? formToken, string? cookiesToken)
        {
            // 檢查空值
            if (formCredential == null || formToken == null && cookiesToken == null)
            {
                return null;
            }

            GoogleJsonWebSignature.Payload? payload;
            try
            {
                // 驗證 token
                if (formToken != cookiesToken)
                {
                    return null;
                }

                // 驗證憑證
                IConfiguration Config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
                string GoogleApiClientId = Config.GetSection("GoogleApiClientId").Value;
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { GoogleApiClientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(formCredential, settings);
                if (!payload.Issuer.Equals("accounts.google.com") && !payload.Issuer.Equals("https://accounts.google.com"))
                {
                    return null;
                }
                if (payload.ExpirationTimeSeconds == null)
                {
                    return null;
                }
                else
                {
                    DateTime now = DateTime.Now.ToUniversalTime();
                    DateTime expiration = DateTimeOffset.FromUnixTimeSeconds((long)payload.ExpirationTimeSeconds).DateTime;
                    if (now > expiration)
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
            return payload;
        }

        [HttpGet]
        public IActionResult SubmitUserInfo()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData.Keep("Email");
                return View();
            }
        }

        [HttpPost]
        public IActionResult SubmitUserInfo(UserInfo model)
        {
            try
            {
                model.Email = TempData["Email"]?.ToString();
                model.UserId = 0;
                if (ModelState.IsValid)
                {
                    var userInfo = new UserInfo
                    {
                        UserId = 0,
                        Name = model.Name,
                        Email = model.Email,
                        Nickname = model.Nickname,
                        Sex = model.Sex,
                        Birthday = model.Birthday,
                        Phone = model.Phone,
                        Status = false
                    };

                    _context.UserInfos.Add(userInfo);
                    _context.SaveChanges();
                    var TempUserId = _context.UserInfos.FirstOrDefault(u => u.Email == model.Email).UserId;
                    var userStatus = new UserStatus
                    {
                        Id = TempUserId,
                        Status = false,
                        Level = 1
                    };
                    _context.UserStatuses.Add(userStatus);
                    _context.SaveChanges();
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    Console.WriteLine("使用者資訊有誤");
                    TempData["SubmitFail"] = "寫入會員資料失敗!請檢查您的資料是否有誤或不符合格式";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["SubmitFail"] = "寫入會員資料失敗!請檢查您的資料是否有誤或不符合格式";
                return View(model);
            }
        }
    }
}

