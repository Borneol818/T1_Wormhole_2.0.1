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
    [Route("api/ViewOthers/[Action]")]
    [ApiController]
    public class ViewOthersApiController : ControllerBase
    {
        private readonly WormHoleContext _db;
        //private readonly IWebHostEnvironment _env;

        public ViewOthersApiController(WormHoleContext db) //IWebHostEnvironment env
        {
            _db = db;
            //_env = env;
        }

        //[Authorize]
        //4/15 晚上改動
        [HttpGet]
        public async Task<IEnumerable<UserInfo>> GetOther(int id) //這是撈使用者資料的
        {
            var result = _db.UserInfos.Where(x => x.UserId == id)
                .Select(e => new UserInfo
                {
                    UserId = e.UserId,
                    Name = e.Name,
                    Nickname = e.Nickname,
                    Phone = e.Phone,
                    Birthday = e.Birthday,
                    SignatureLine = e.SignatureLine,
                    Sex = e.Sex,
                    Photo = null,
                    Wallet = e.Wallet,
                });
            return result;
        }
        [HttpGet]
        public async Task<IEnumerable<UserStatus>> GetOtherStatus(int id)
        {
            var result = _db.UserStatuses.Where(x => x.Id == id);
            return result;
        }
        [HttpGet]
        public async Task<FileResult> GetPhoto(int id) //這是撈使用者頭像的(傳回的是單個檔案用File)
        {
            string fileName = Path.Combine("wwwroot", "images", "PhotoTest.jpg");
            UserInfo e = await _db.UserInfos.FindAsync(id);
            byte[] ImageContent = e?.Photo != null ? e.Photo : System.IO.File.ReadAllBytes(fileName);
            return File(ImageContent, "image/jpeg");
        }
        //Borneol 04/19 撈Obtain status 資料表中符合UserId的資料，再依照撈出的資料去撈Obtains中找到該Obtaib的圖片來顯示
        [HttpGet]
        public async Task<List<string>> GetOtherBadgePicture(int id) //這是撈徽章的
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
        public async Task<List<string>> GetOtherAchievementPicture(int id) //這是撈成就的
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

        //Borneol 04/29 使用者PO文&留言歷史
        [HttpGet]
        public async Task<List<ArticleDTO>> GetOtherArticlesHistory(int id)
        {
            
            var result = _db.Articles.Where(x => x.WriterId == id)
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
        public async Task<List<CommentDto>> GetOtherCommentHistory(int id)
        {
            
            var ArtTitles = _db.Articles.Select(e => new { e.Title, e.ArticleId });
            var commentHistory = _db.ArticleResponses.Where(x => x.UserId == id)
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
        //Borneol 04/29 使用者PO文&留言歷史
    }
}
