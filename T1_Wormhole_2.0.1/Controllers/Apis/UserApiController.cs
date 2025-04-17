using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;


namespace T1_Wormhole_2._0._1.Controllers.Apis
{
    [Route("api/User/[Action]")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly WormHoleContext _db;
        //private readonly IWebHostEnvironment _env;

        public UserApiController(WormHoleContext db) //IWebHostEnvironment env
        {
            _db = db;
            //_env = env;
        }

        //4/15 晚上改動
        [HttpGet]
        public async Task<IEnumerable<UserInfo>> Get(int id)
        {
            var result = _db.UserInfos.Where(x => x.UserId == id)
                .Select(e => new UserInfo
                {
                    UserId = e.UserId,
                    Name = e.Name,
                    Nickname = e.Nickname,
                    Phone = e.Phone,
                    Brithday = e.Brithday,
                    SignatureLine = e.SignatureLine,
                    Sex = e.Sex,
                    Photo = null,
                });
            return result;
        }
        [HttpGet]
        public async Task<IEnumerable<UserStatus>> GetStatus(int id)
        {
            var result = _db.UserStatuses.Where(x => x.Id == id);
            return result;
        }

        [HttpGet]
        public async Task<FileResult> GetPhoto(int id)
        {
            string fileName = Path.Combine("wwwroot", "images", "PhotoTest.jpg");
            UserInfo e = await _db.UserInfos.FindAsync(id);
            byte[] ImageContent = e?.Photo != null ? e.Photo : System.IO.File.ReadAllBytes(fileName);
            return File(ImageContent, "image/jpeg");
        }

        [HttpPost]
        public async Task<string> Edit(UserInfoDto model)
        {
            UserInfo data = await _db.UserInfos.FindAsync(model.UserId);
            if (data == null)
            {
                return "找不到會員資料";
            }
            data.Name = model.Name;
            data.Nickname = model.Nickname;
            data.Brithday = DateOnly.Parse(model.Birthday);
            data.SignatureLine = model.Signature;
            data.Sex = model.Sex;
            //data.Phone = model.Phone;
            if (model.Photo != null)
            {
                using (BinaryReader br = new BinaryReader(model.Photo.OpenReadStream()))
                {
                    data.Photo = br.ReadBytes((int)model.Photo.Length);
                }
            }

            _db.Entry(data).State = EntityState.Modified;

            //_db.Update(data);
            await _db.SaveChangesAsync();
            return (data.ToString());
        }
    }
}
