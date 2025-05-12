using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Xml;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Security.Claims;

namespace T1_Wormhole_2._0._1.Controllers.Apis
{
    [Route("api/Poster/[action]")]
    [ApiController]
    public class PosterApiController : ControllerBase
    {
        private readonly WormHoleContext _db;

        public PosterApiController(WormHoleContext db)
        {
            _db = db;
        }

        // Post: api/Poster/postArticle
        [HttpPost]
        public bool postArticle(AddDiscussArticlesDTO DTOModel)
        {
            try
            {
                //判斷身分，存News OR Discuss文章
                if (User.FindFirst(ClaimTypes.Role)?.Value == "User")
                {
                    Article Art = new Article
                    {
                        Title = DTOModel.Title,
                        Type = true,//讀取是否為使用者(使用者=文章=true)
                        CreateTime = DateTime.Now,
                        Content = DTOModel.Content,
                        WriterId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),//讀取使用者ID
                    };
                    if (DTOModel.ArticleCover != null)
                    {
                        using (BinaryReader br = new BinaryReader(DTOModel.ArticleCover.OpenReadStream()))
                        {
                            Art.ArticleCover = br.ReadBytes((int)DTOModel.ArticleCover.Length);
                        }
                    }
                    _db.Articles.Add(Art);
                    _db.SaveChanges();

                    return true;
                }
                else if(User.FindFirst(ClaimTypes.Role)?.Value == "Admin")
                {
                    Article Art = new Article
                    {
                        Title = DTOModel.Title,
                        Type = false,
                        CreateTime = DateTime.Now,
                        Content = DTOModel.Content,
                        ReleaseBy = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),//讀取管理員ID
                    };
                    if (DTOModel.ArticleCover != null)
                    {
                        using (BinaryReader br = new BinaryReader(DTOModel.ArticleCover.OpenReadStream()))
                        {
                            Art.ArticleCover = br.ReadBytes((int)DTOModel.ArticleCover.Length);
                        }
                    }
                    _db.Articles.Add(Art);
                    _db.SaveChanges();

                    return true;
                }
                
                return false;

            }
            catch (Exception)
            {
                return false;
            }

        }

        //呼叫簽名檔
        public IEnumerable<string> getSignature()
        {
            var userID = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = _db.UserInfos.Where(x => x.UserId == userID).Select(x => x.SignatureLine);
            return result;
        }


        //編輯文章，將ID儲存於LocalStorage，待轉頁面後呼叫
        // GET: api/Poster/GetEditArticle/6
        [HttpGet("{id}")]
        public async Task<AddDiscussArticlesDTO> GetEditArticle(int id)
        {
            var Article = await _db.Articles.FindAsync(id) ;

            if (Article == null)
            {
                return null;
            }
            //暫不做簽名檔處理(暫時視為原簽名檔是文章內容一部份)
            //(需要在資料庫article新增簽名檔欄位之後讀取處理)
            AddDiscussArticlesDTO DTOModel = new AddDiscussArticlesDTO
            {
                Title = Article.Title,
                Type = Article.Type,//讀取是否為使用者
                CreateTime = Article.CreateTime,
                Content = Article.Content,
            };

            
            return DTOModel;
        }

        //api/Poster/EditArticle/6
        //編輯文章，送出編輯內容
        [HttpPut("{id}")]
        public async Task<string> EditArticle(int id, AddDiscussArticlesDTO DTOModel)
        {
            var Article = await _db.Articles.FindAsync(id);


                Article.Title = DTOModel.Title;
                Article.Content = DTOModel.Content;

            if (DTOModel.ArticleCover != null)
            {
                using (BinaryReader br = new BinaryReader(DTOModel.ArticleCover.OpenReadStream()))
                {
                    Article.ArticleCover = br.ReadBytes((int)DTOModel.ArticleCover.Length);
                }
            }


            _db.Entry(Article).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
                {
                    return "文章修改失敗";
                }
                else
                {
                    throw;
                }
            }

            return "文章修改成功";
        }

        //確保article存在用
        private bool ArticleExists(int id)
        {
            return _db.Articles.Any(e => e.ArticleId == id);
        }

        [HttpGet]
        public async Task<FileResult> GetPhoto(int id)
        {
            string fileName = Path.Combine("wwwroot", "images", "noimages.jpg");
            Article e = await _db.Articles.FindAsync(id);
            byte[] ImageContent = e?.ArticleCover != null ? e.ArticleCover : System.IO.File.ReadAllBytes(fileName);
            return File(ImageContent, "image/jpeg");
        }
    }
}
