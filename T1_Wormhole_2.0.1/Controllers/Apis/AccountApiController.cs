using System.Data;
using System.Security.Claims;
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

        [HttpGet]
        public bool isAdmin() 
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Admin")
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        [HttpGet]
        public IActionResult GetUser()
        {
            var users = _context.UserInfos.Select(
                r=>new
                {
                    UserId = r.UserId,
                    Email = r.Email,
                    Name = r.Name,
                    Nickname = r.Nickname,
                    Sex = r.Sex,
                    Phone = r.Phone,
                    Birthday = r.Birthday,
                    Status = r.Status

                }).ToList();
            return Ok(users);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] UserInfo  model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 根據 UserId 查找要更新的使用者
            var userToUpdate = await _context.UserInfos.FindAsync(model.UserId);

            if (userToUpdate == null)
            {
                return NotFound($"找不到 ID 為 {model.UserId} 的使用者。");
            }

            // 更新使用者的屬性 (只更新前端傳送過來的屬性)
            if (model.Name != null)
            {
                userToUpdate.Name = model.Name;
            }
            if (model.Email != null)
            {
                userToUpdate.Email = model.Email;
            }
            if (model.Nickname != null)
            {
                userToUpdate.Nickname = model.Nickname;
            }
            if (model.Phone != null)
            {
                userToUpdate.Phone = model.Phone;
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok("使用者資料已成功更新。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新使用者資料時發生錯誤: {ex.Message}");
                return StatusCode(500, "更新使用者資料時發生伺服器錯誤。");
            }
        }

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteUser(int id)
        //{
            //var userToDelete = await _context.UserInfos.FindAsync(id);
            //if (userToDelete == null)
            //{
                //return NotFound($"找不到 ID 為 {id} 的使用者。");
            //}

            //try
            //{
                //_context.UserInfos.Remove(userToDelete);
                //await _context.SaveChangesAsync();
                //return Ok($"ID 為 {id} 的使用者已成功刪除。");
            //}
            //catch (Exception ex)
            //{
                //Console.WriteLine($"刪除使用者時發生錯誤: {ex.Message}");
                //return StatusCode(500, "刪除使用者時發生伺服器錯誤。");
            //}
        //}
    }
}
