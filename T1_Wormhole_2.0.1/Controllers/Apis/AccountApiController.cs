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

        public AccountApiController(WormHoleContext context)
        {
            _context = context;
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
            var userIdentifier = HttpContext.Request.Cookies["LoginName"];

            if (string.IsNullOrEmpty(userIdentifier))
            {
                return "";
            }
            var UserIdentifier = userIdentifier;
            return UserIdentifier;
        }

        [HttpGet]
        public string GetPassword()
        {
            var pwd = HttpContext.Request.Cookies["LoginPassword"];

            if (string.IsNullOrEmpty(pwd))
            {
                return "";
            }
            var Password = pwd;
            return Password;
        }
    }
}
