using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T1_Wormhole_2._0._1.Models.Database;

namespace T1_Wormhole_2._0._1.Controllers.Apis
{
    //修改API Controller的Route
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly WormHoleContext _context;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public AccountApiController(WormHoleContext context, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _dataProtectionProvider = dataProtectionProvider;
        }

        [HttpGet]
        public bool GetAccount(string Account)
        {
            var ExistAccount = _context.Logins.FirstOrDefault(x => x.Account == Account);
            return (ExistAccount == null);
        }

        [HttpGet]
        public string GetLoginName()
        {
            var encryptedUserIdentifier = HttpContext.Request.Cookies["LoginName"];

            if (string.IsNullOrEmpty(encryptedUserIdentifier))
            {
                return "";
            }
            try
            {
                var NameProtector = _dataProtectionProvider.CreateProtector("LoginNameCookie");
                string UserIdentifier = NameProtector.Unprotect(encryptedUserIdentifier);
                return UserIdentifier;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting cookie: {ex.Message}");
                return "";
            }
        }

        [HttpGet]
        public string GetPassword()
        {
            var encryptedPWD = HttpContext.Request.Cookies["LoginPassword"];

            if (string.IsNullOrEmpty(encryptedPWD))
            {
                return "";
            }
            try
            {
                var PWDProtector = _dataProtectionProvider.CreateProtector("LoginPWDCookie");
                string Password = PWDProtector.Unprotect(encryptedPWD);
                return Password;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting cookie: {ex.Message}");
                return "";
            }
        }
    }
}
