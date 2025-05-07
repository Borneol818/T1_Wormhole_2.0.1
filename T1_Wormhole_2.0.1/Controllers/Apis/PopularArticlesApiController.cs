using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace T1_Wormhole_2._0._1.Controllers.Apis
{
    [Route("api/PopularArticles/[action]")]
    [ApiController]
    public class PopularArticlesApiController : ControllerBase
    {
        private readonly WormHoleContext _db;

        public PopularArticlesApiController(WormHoleContext db)
        {
            _db = db;
        }

        [HttpGet]
        public List<RatingsDTOs> Get() {
            //得到近期日期文章-讀取7日內最熱門文章
            var RecentArticlesID = _db.Articles.Where(x =>x.CreateTime.AddDays(7)>=DateTime.Now).Select(x => x.ArticleId);

            List<RatingsDTOs> RatingsDTO = new List<RatingsDTOs>();

            //var a = _db.Articles.Where(x => x.ArticleId == 1).Select(x => x.WriterId).First();
            //參考mvc core 範例 viewTwoTable
            foreach (int id in RecentArticlesID.ToList())
            {
                var idsRating=_db.Ratings.Where(x => x.ArticleId == id);
                var prCount= idsRating.Sum(x=>x.PositiveRating);
                var ngrCount= idsRating.Sum(x=>x.NegativeRating);
                var popularCount=prCount+ngrCount;
                var forTitle=_db.Articles.Where(x=>x.ArticleId==id).Select(x=>x.Title).First();

                int? writerId;
                int? managersId;
                string name;
                bool type;

                if (_db.Articles.Where(x => x.ArticleId == id).Select(x => x.WriterId).First()!=null)
                {
                     writerId= _db.Articles.Where(x => x.ArticleId == id).Select(x => x.WriterId).First();
                     name= _db.UserInfos.Where(x => x.UserId == writerId).Select(x => x.Nickname).First();
                     type = _db.Articles.Where(x=> x.ArticleId == id).Select(x=>x.Type).First();
                }
                else {
                     managersId= _db.Articles.Where(x => x.ArticleId == id).Select(x => x.ReleaseBy).First();
                     name = _db.BoManagers.Where(x => x.ManagerId == managersId).Select(x => x.Name).First();
                     type = _db.Articles.Where(x => x.ArticleId == id).Select(x => x.Type).First();
                }

                RatingsDTO.Add(new RatingsDTOs
                {
                    articlesId = id,
                    prCount = prCount,
                    ngrCount = ngrCount,
                    popularCount = popularCount,
                    articleTitle = forTitle,
                    Name = name,
                    Type = type,
                });
            }
            if (RatingsDTO.Count>6) 
            {
                RatingsDTO = RatingsDTO.OrderByDescending(n => n.popularCount).Take(6).ToList();
            }
            

            return RatingsDTO;
        }
    }
}
