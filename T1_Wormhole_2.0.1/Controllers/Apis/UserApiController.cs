using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Claims;
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

        [Authorize]
        //4/15 晚上改動
        [HttpGet]
        public async Task<IEnumerable<UserInfo>> Get() //這是撈使用者資料的
        {
            var currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            //if (currentUserId != id)
            //{
            //    return Enumerable.Empty<UserInfo>(); //先回傳空列舉,看要回傳自己的資料還是怎麼樣
            //}
            var result = _db.UserInfos.Where(x => x.UserId == currentUserId)
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
        public async Task<IEnumerable<UserStatus>> GetStatus()
        {
            var currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = _db.UserStatuses.Where(x => x.Id == currentUserId);
            return result;
        }

        [HttpGet]
        public async Task<int?> updateCoins() //這是計算使用者的錢包餘額的
        {
            var currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = _db.ForumCoins.Where(x => x.UserId == currentUserId && x.Status.Contains("已發放"));                
            var totalCoins = result.Sum(x => x.CoinAmount);
            var user =await _db.UserInfos.FindAsync(currentUserId);            
            
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

        //[HttpGet]
        //public IActionResult GetUserCoins()
        //{
        //    var currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    int? Wallet = _db.ObtainStatuses.Where(x => x.UserId == currentUserId).Select(x => x.Count).Sum();
            
        //    return Ok(Wallet);

        //}


        [HttpGet]
        public async Task<FileResult> GetPhoto(int id) //這是撈使用者頭像的(傳回的是單個檔案用File)
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
        public async Task<List<string>> GetBadgePicture() //這是撈徽章的
        {
            var currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var fileName = new List<string>();
            var obtains = new List<string>();
            var findUserObtain = await _db.ObtainStatuses
                .Where(x => x.UserId == currentUserId && x.Status.Contains("使用中")).ToListAsync();
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
                            ShowPicture = Convert.ToBase64String(y.Picture), //因為要傳回多個圖片不能用File，所以將其轉型為Base64
                        }).FirstOrDefaultAsync();
                    if (obtain != null)
                    {
                        obtains.Add($"data:image/jpeg;base64,{obtain.ShowPicture}"); //把Base64的圖片加上data:image/jpeg;base64,這個前綴，並依次塞入obtains
                    }

                }
                return obtains;
            }
            fileName.Add(Path.Combine("wwwroot", "images", "PhotoTest.jpg"));
            return fileName;
        }


        [HttpGet]
        public async Task<List<string>> GetAchievementPicture() //這是撈成就的
        {
            var currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var fileName = new List<string>();
            var obtains = new List<string>();
            var findUserObtain = await _db.ObtainStatuses
                .Where(x => x.UserId == currentUserId && x.Status.Contains("使用中")).ToListAsync();
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
                            ShowPicture = Convert.ToBase64String(y.Picture), //因為要傳回多個圖片不能用File，所以將其轉型為Base64
                        }).FirstOrDefaultAsync();
                    if (obtain != null)
                    {
                        obtains.Add($"data:image/jpeg;base64,{obtain.ShowPicture}"); //把Base64的圖片加上data:image/jpeg;base64,這個前綴，並依次塞入obtains
                    }

                }
                return obtains;
            }
            fileName.Add(Path.Combine("wwwroot", "images", "PhotoTest.jpg"));
            return fileName;
        }

        //Borneol 04/19 撈Obtain status 資料表中符合UserId的資料，再依照撈出的資料去撈Obtains中找到該Obtaib的圖片來顯示

        //Borneol 04/24 使用者升級判定-製作中
        //依照瀏覽文章次數、註冊天數、評論數量、發文數量等決定user等級
        //(等級如何評斷可以自己設計，不一定這邊舉例都要用到)
        [NonAction]
        public void UserLevelUp() {
            int level = 0;
            var levelExp = EachLevelExp(); // 產生等級門檻，例如第 N 級需要 N^2 * 10
            var userStatus = _db.UserStatuses.ToList();
            foreach (var item in userStatus)
            {
                //這是我初建時跑邏輯的參考而已
                //if (item.ReadCount >= 5 && item.PostCount >= 1 || item.CommentCount >= 3 && item.PostCount >= 1)
                //{
                //    level = 5;
                //}
                //else if (item.ReadCount >= 1 && item.PostCount >= 1 && item.CommentCount >= 1)
                //{
                //    level = 4;
                //}
                //else if (item.ReadCount >= 1 && item.PostCount >= 1 || item.CommentCount >= 1 && item.PostCount >= 1)
                //{
                //    level = 3;
                //}
                //else if (item.ReadCount >= 1 || item.CommentCount >= 1 || item.PostCount >= 1)
                //{
                //    level = 2;
                //}
                //else
                //{
                //    level = 1;
                //}
                //這是我初建時跑邏輯的參考而已

                //這邊是計算經驗值的
                int? exp = item.ReadCount * 1 + item.PostCount * 5 + item.CommentCount * 2;
                //這邊是計算經驗值的

                //這邊是計算等級的
                for (int i = levelExp.Count - 1; i >= 0; i--)
                {
                    if (exp >= levelExp[i])
                    {
                        level = i + 1;
                        break;
                    }
                }
                //這邊是計算等級的
                item.Level = level;
                _db.Entry(item).State = EntityState.Modified;                
                //這邊可以加入升級的通知或其他操作
            }
            _db.SaveChanges();
        }
        private List<int> EachLevelExp(int maxLevel = 99) //這邊是計算每個等級所需的經驗值(最高99)
        {
            var eachLevelExp = new List<int>();
            for (int i = 1; i <= maxLevel; i++)
            {
                eachLevelExp.Add((int)Math.Pow(i, 2) * 3); //等級= i, Math.Pow=i², 經驗值=(i² × 10), e.g. Lv2 = 3, Lv3 = 12, Lv4 = 27, ...
            }
            return eachLevelExp;
        }
        //Borneol 04/24 使用者升級判定-製作中

        //Borneol 04/29 使用者PO文&留言歷史-製作中
        [HttpGet]
        public async Task<List<ArticleDTO>> GetArticlesHistory()
        {
            var currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = _db.Articles.Where(x => x.WriterId == currentUserId)
                .Select(e => new ArticleDTO
                {
                    ArticleID = e.ArticleId,
                    Title = e.Title,
                    Type = e.Type,
                    //CreateTime = e.CreateTime,
                    Content = e.Content,                    
                });
            return result.ToList();
        }
        [HttpGet]
        public async Task<List<CommentDto>> GetCommentHistory()
        {
            var currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var ArtTitles= _db.Articles.Select(e =>new { e.Title, e.ArticleId});
            var commentHistory = _db.ArticleResponses.Where(x => x.UserId == currentUserId)
                .Select(e => new CommentDto
                {                    
                    ArticleID = e.ArticleId,
                    Id = e.Id,
                    Comment = e.Comment,
                    //CreateTime = e.CreateTime,
                }).ToList();
            foreach (var item in commentHistory)
            {
                var article = ArtTitles.FirstOrDefault(x => x.ArticleId == item.ArticleID);
                if (article != null)
                {
                    item.Title = article.Title;
                }
            }
            return commentHistory;
        }
        //Borneol 04/29 使用者PO文&留言歷史-製作中
    }
}
