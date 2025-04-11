using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Xml;

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
                        _db.Articles.Add(new Article
                        {
                            Title = DTOModel.Title,
                            Type = true,//讀取是否為使用者
                            CreateTime = DateTime.Now,
                            Content = DTOModel.Content + "\n" + DTOModel.Signature[0],
                            WriterId = DTOModel.WriterID,//讀取使用者ID
                        });
                        _db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _db.Articles.Add(new Article
                        {
                            Title = DTOModel.Title,
                            Type = true,//讀取是否為使用者
                            CreateTime = DateTime.Now,
                            Content = DTOModel.Content,
                            WriterId = DTOModel.WriterID,//讀取使用者ID
                        });
                        _db.SaveChanges();
                        return true;
                    }
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
    }
}
