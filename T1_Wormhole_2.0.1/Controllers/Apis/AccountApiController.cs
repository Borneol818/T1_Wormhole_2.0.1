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
    }
}
