using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        [HttpPost]
        public bool postArticle(AddDiscussArticlesDTO DTOModel)
        {
            try
            {
                if (DTOModel.Signature != null)
                {
                    if (DTOModel.Signature.Length >= 1)
                    {
                        Article Art= new Article
                        {
                            Title = DTOModel.Title,
                            Type = true,//讀取是否為使用者
                            CreateTime = DateTime.Now,
                            Content = DTOModel.Content + "\n" + DTOModel.Signature[0],
                            WriterId = DTOModel.WriterID,//讀取使用者ID
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
                    //else
                    //{
                    //    Article Art = new Article
                    //    {
                    //        Title = DTOModel.Title,
                    //        Type = true,//讀取是否為使用者
                    //        CreateTime = DateTime.Now,
                    //        Content = DTOModel.Content,
                    //        WriterId = DTOModel.WriterID,//讀取使用者ID
                    //    };
                    //    if (DTOModel.ArticleCover != null)
                    //    {
                    //        using (BinaryReader br = new BinaryReader(DTOModel.ArticleCover.OpenReadStream()))
                    //        {
                    //            Art.ArticleCover = br.ReadBytes((int)DTOModel.ArticleCover.Length);
                    //        }
                    //    }

                    //    _db.Articles.Add(Art);
                    //    _db.SaveChanges();
                    //    return true;
                    //}
                    return false;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                return false;
            }

        }

        public IEnumerable<string> getSignature(int userID)
        {
            var result = _db.UserInfos.Where(x => x.UserId == userID).Select(x => x.SignatureLine);
            return result;
        }

        [HttpGet]
        public async Task<FileResult> GetPhoto(int id)
        {
            string fileName = Path.Combine("wwwroot", "images", "PhotoTest.jpg");
            Article e = await _db.Articles.FindAsync(id);
            byte[] ImageContent = e?.ArticleCover != null ? e.ArticleCover : System.IO.File.ReadAllBytes(fileName);
            return File(ImageContent, "image/jpeg");
        }

        
    }
}
