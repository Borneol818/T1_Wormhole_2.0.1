using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T1_Wormhole_2._0._1.Models.Database;
using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace T1_Wormhole_2._0._1.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly WormHoleContext _context;

        public CommentsController(WormHoleContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetByArticleId(int articleId)
        {
            var comments = _context.ArticleResponses
            .Where(r => (r.ArticleId == articleId))
            .Select(r => new
            {
                 writer = r.User.Nickname,
                 content = r.Comment,
                date = r.CreateTime.ToString("yyyy年MM月dd日 HH點mm分ss秒")
            }).ToList();


            return Ok(comments);
        }
        [HttpGet]
        public IActionResult GetRating(int articleId)
        {
            
            int? Ratingcount = _context.Ratings.Where(r => r.ArticleId == articleId).Select(x=>x.PositiveRating).Sum();
            int? nRatingcount = _context.Ratings.Where(r => r.ArticleId == articleId).Select(x => x.NegativeRating).Sum();
            //int?[] Total =new int?[] {Ratingcount, nRatingcount };
            //Console.Write(Total);
            return Ok(new
            {
                Positive = Ratingcount ?? 0,
                Negative = nRatingcount ?? 0
            });
            
        }
    }
}
