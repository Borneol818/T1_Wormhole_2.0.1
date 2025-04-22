using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        public async Task<IEnumerable<UserInfo>> Get(int id) //這是撈使用者資料的
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
                    Wallet = e.Wallet,
                });
            return result;
        }
        [HttpGet]
        public async Task<IEnumerable<UserStatus>> GetStatus(int id) //這是撈使用者狀態的
        {
            var result = _db.UserStatuses.Where(x => x.Id == id);
            return result;
        }

        [HttpGet]
        public async Task<int?> updateCoins(int id) //這是計算使用者的錢包餘額的
        {
            var result = _db.ForumCoins.Where(x => x.UserId == id && x.Status.Contains("已發放"));
            var totalCoins = result.Sum(x => x.CoinAmount);
            var user = await _db.UserInfos.FindAsync(id);

            if (user != null)
            {
                user.Wallet = totalCoins;
                await _db.SaveChangesAsync();
                return user.Wallet;
            }
            else
            {
                return null;
            }
        }

        

        [HttpGet]
        public async Task<FileResult> GetPhoto(int id) //這是撈使用者頭像的(傳回的是單個檔案用File)
        {
            string fileName = Path.Combine("wwwroot", "images", "PhotoTest.jpg");
            UserInfo e = await _db.UserInfos.FindAsync(id);
            byte[] ImageContent = e?.Photo != null ? e.Photo : System.IO.File.ReadAllBytes(fileName);
            return File(ImageContent, "image/jpeg");
        }

        [HttpPost]
        public async Task<string> Edit(UserInfoDto model) //這是使用者資料編輯
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
            data.Phone = model.Phone;
            if (model.Photo != null) //這是上傳圖片用的
            {
                using (BinaryReader br = new BinaryReader(model.Photo.OpenReadStream()))
                {
                    data.Photo = br.ReadBytes((int)model.Photo.Length);
                }
            }

            _db.Entry(data).State = EntityState.Modified;

            //_db.Update(data);
            await _db.SaveChangesAsync();
            return "修改成功";
        }

        //Borneol 04/19 撈Obtain status 資料表中符合UserId的資料，再依照撈出的資料去撈Obtains中找到該Obtaib的圖片來顯示
        [HttpGet]
        public async Task<List<string>> GetBadgePicture(int id) //這是撈徽章的
        {

            var fileName = new List<string>();
            var obtains = new List<string>();
            var findUserObtain = await _db.ObtainStatuses
                .Where(x => x.UserId == id && x.Status.Contains("使用中")).ToListAsync();
            if (findUserObtain != null)
            {
                foreach (var item in findUserObtain)
                {
                    var obtain = await _db.Obtains
                        .Where(y => y.ObtainId == item.ObtainId && y.Type == 2) //用這裡的Type決定留下的是徽章
                        .Select(y => new ObtainDTO
                        {
                            ObtainID = y.ObtainId,
                            Type = y.Type,
                            Name = y.Name,
                            Picture = Convert.ToBase64String(y.Picture), //因為要傳回多個圖片不能用File，所以將其轉型為Base64
                        }).FirstOrDefaultAsync();
                    if (obtain != null)
                    {
                        obtains.Add($"data:image/jpeg;base64,{obtain.Picture}"); //把Base64的圖片加上data:image/jpeg;base64,這個前綴，並依次塞入obtains
                    }

                }
                return obtains;
            }
            fileName.Add(Path.Combine("wwwroot", "images", "PhotoTest.jpg"));
            return fileName;
        }


        [HttpGet]
        public async Task<List<string>> GetAchievementPicture(int id) //這是撈成就的
        {

            var fileName = new List<string>();
            var obtains = new List<string>();
            var findUserObtain = await _db.ObtainStatuses
                .Where(x => x.UserId == id && x.Status.Contains("使用中")).ToListAsync();
            if (findUserObtain != null)
            {
                foreach (var item in findUserObtain)
                {
                    var obtain = await _db.Obtains
                        .Where(y => y.ObtainId == item.ObtainId && y.Type == 1) //用這裡的Type決定留下的是成就
                        .Select(y => new ObtainDTO
                        {
                            ObtainID = y.ObtainId,
                            Type = y.Type,
                            Name = y.Name,
                            Picture = Convert.ToBase64String(y.Picture), //因為要傳回多個圖片不能用File，所以將其轉型為Base64
                        }).FirstOrDefaultAsync();
                    if (obtain != null)
                    {
                        obtains.Add($"data:image/jpeg;base64,{obtain.Picture}"); //把Base64的圖片加上data:image/jpeg;base64,這個前綴，並依次塞入obtains
                    }

                }
                return obtains;
            }
            fileName.Add(Path.Combine("wwwroot", "images", "PhotoTest.jpg"));
            return fileName;
        }

        //Borneol 04/19 撈Obtain status 資料表中符合UserId的資料，再依照撈出的資料去撈Obtains中找到該Obtaib的圖片來顯示
    }
}
