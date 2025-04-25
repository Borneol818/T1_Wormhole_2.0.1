using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;

namespace T1_Wormhole_2._0._1.Controllers.Apis
{
    [Route("api/Search/[Action]")]
    [ApiController]
    public class SearchApiController : ControllerBase
    {
        private readonly WormHoleContext _db;
        public SearchApiController(WormHoleContext db) //IWebHostEnvironment env
        {
            _db = db;
            //_env = env;
        }
        
        [HttpPost]
        public async Task<IEnumerable<SearchDTO>> TestU([FromBody] SearchDTO searchDto) //這是搜尋全站文章與使用者
        {            
            var resultUser = _db.UserInfos.Where(
                x => x.UserId == searchDto.ResultUserId ||
                x.Name.Contains(searchDto.UserName) ||
                x.Nickname.Contains(searchDto.UserNickname))
                .Select(x => new SearchDTO
                {
                    ResultUserId = x.UserId,                    
                    UserNickname = x.Nickname,
                    UserSignature = x.SignatureLine,
                    UserPhoto = null,
                });
            
            return resultUser;
        }
        [HttpPost]
        public async Task<IEnumerable<SearchDTO>> TestA([FromBody] SearchDTO searchDto) //這是搜尋全站文章與使用者
        {
            var resultArticle = _db.Articles.Where(
                x => x.ArticleId == searchDto.ResultArticleId ||
                x.Title.Contains(searchDto.ArticleTitle))
                .Select(x => new SearchDTO
                {
                    ResultArticleId = x.ArticleId,
                    ArticleTitle = x.Title,
                    ArticleContent = x.Content,
                    //ArticleWriterId = x.WriterId,
                    ArticleCover = null,
                });           

            return resultArticle;
        }
        [HttpGet]
        public async Task<FileResult> GetUserPhoto(int id) //這是撈使用者頭像的(傳回的是單個檔案用File)
        {
            string fileName = Path.Combine("wwwroot", "images", "PhotoTest.jpg");
            UserInfo e = await _db.UserInfos.FindAsync(id);
            byte[] ImageContent = e?.Photo != null ? e.Photo : System.IO.File.ReadAllBytes(fileName);
            return File(ImageContent, "image/jpeg");
        }
        [HttpGet]
        public async Task<FileResult> GetArtCover(int id) //這是撈使用者頭像的(傳回的是單個檔案用File)
        {
            string fileName = Path.Combine("wwwroot", "images", "PhotoTest.jpg");
            Article e = await _db.Articles.FindAsync(id);
            byte[] ImageContent = e?.ArticleCover != null ? e.ArticleCover : System.IO.File.ReadAllBytes(fileName);
            return File(ImageContent, "image/jpeg");
        }
    }
}
