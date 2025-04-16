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
        public UserApiController(WormHoleContext db)
        {
            _db = db;
        }

        //[HttpGet]
        //public IEnumerable<UserInfo> Get(int id)
        //{

        //    //var result = _db.UserInfos.Where(x => x.Name == "林克").Select(x => new UserInfoDto
        //    //{
        //    //    Id = x.UserId,
        //    //    Name = x.Name,
        //    //}).ToList();

        //    var result = _db.UserInfos.Where(x => x.UserId == id);
        //    return result;
        //}
        //[HttpGet]
        //public IEnumerable<UserStatus> GetStatus(int id)
        //{
        //    var result = _db.UserStatuses.Where(x => x.Id == id);
        //    return result;
        //}

        ////[HttpGet]
        ////public string Get(int id)
        ////{
        ////    UserInfo? p = _db.UserInfos.Find(id);
        ////    string? imageUrl = p?.Photo;
        ////    return imageUrl;
        ////}

        //[HttpPost]
        //public bool Edit(UserInfoDto model)
        //{
        //    var data = _db.UserInfos.FirstOrDefault(x => x.UserId == model.UserId);
        //    if (data == null)
        //    {
        //        return false;
        //    }
        //    data.Name = model.Name;
        //    data.Nickname = model.Nickname;
        //    data.Phone = model.Phone;
        //    data.Brithday = DateOnly.Parse(model.Birthday);
        //    data.SignatureLine = model.Signature;
        //    data.Sex = model.Sex;
        //    //_db.Update(data);
        //    _db.SaveChanges();
        //    return true;

        //    //try
        //    //{
        //    //    _db.UserInfos.Add(new UserInfo
        //    //    {
        //    //        Name = model.Name
        //    //    });
        //    //    _db.SaveChanges();
        //    //    return true;
        //    //}
        //    //catch (Exception)
        //    //{
        //    //    return false;
        //    //}
        //}

        ////public bool AddPhoto(int id,[Bind("Photo")] UserInfo userInfo) {
        ////    if (ModelState.IsValid) {
        ////        try
        ////        {
        ////            UserInfo? p = _db.UserInfos.Find(id);
        ////            if (p.Photo != null)
        ////            {
        ////                ReadUploadImage(userInfo);

        ////            }
        ////            else
        ////            {
        ////                userInfo.Photo = p.Photo;
        ////            }
        ////            _db.Entry(p).State = EntityState.Detached;
        ////            _db.Update(userInfo);
        ////            _db.SaveChanges();
        ////            return true;
        ////        }
        ////        catch (DbUpdateConcurrencyException)
        ////        {
        ////            if (!UserInfoExists(userInfo.UserId))
        ////            {
        ////                return false;
        ////            }
        ////            else
        ////            {
        ////                throw;
        ////            }
        ////        }

        ////    }
        ////    return false;
        ////}

        ////private bool UserInfoExists(int id)
        ////{
        ////    return _db.UserInfos.Any(e => e.UserId == id); ;
        ////}

        ////private void ReadUploadImage(UserInfo userInfo)
        ////{
        ////    if (Request.Form.Files["Photo"] != null)
        ////    {
        ////        using (BinaryReader br=new BinaryReader(
        ////            Request.Form.Files["Photo"].OpenReadStream()))
        ////        {
        ////            userInfo.Photo = br.ReadBytes((int)Request.Form.Files["Photo"].Length);
        ////        }
        ////    }
        ////}
        ////[HttpPost]
        ////public bool AddPhoto(UserInfoDto model)
        ////{
        ////    var data = _db.UserInfos.FirstOrDefault(y => y.UserId == model.UserId);
        ////    if (data == null)
        ////    {
        ////        return false;
        ////    }
        ////    data.Photo = model.Photo;
        ////    _db.SaveChanges();
        ////    return true;
        ////}

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

        [HttpGet("GetPhoto/{id}")]
        public async Task<FileResult> GetPhoto(int id)
        {
            string fileName = Path.Combine("wwwroot", "images", "200Test.jpg");
            UserInfo e = await _db.UserInfos.FindAsync(id);
            byte[]? ImageContent = e?.Photo != null ? e.Photo : System.IO.File.ReadAllBytes(fileName);
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
