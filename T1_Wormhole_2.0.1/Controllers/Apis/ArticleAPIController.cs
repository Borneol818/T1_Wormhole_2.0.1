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

            int? Ratingcount = _context.Ratings.Where(r => r.ArticleId == articleId).Select(x => x.PositiveRating).Sum();
            int? nRatingcount = _context.Ratings.Where(r => r.ArticleId == articleId).Select(x => x.NegativeRating).Sum();
            //int?[] Total =new int?[] {Ratingcount, nRatingcount };
            //Console.Write(Total);
            return Ok(new
            {
                Positive = Ratingcount ?? 0,
                Negative = nRatingcount ?? 0
            });

        }
        [HttpGet]
        public async Task<FileResult> GetArticlePhoto(int articleId)
        {
            string fileName = Path.Combine("wwwroot", "images", "PhotoTest.jpg");
            Article e = await _context.Articles.FindAsync(articleId);
            byte[] ImageContent = e?.Picture != null ? e.Picture : System.IO.File.ReadAllBytes(fileName);
            return File(ImageContent, "image/jpeg");
        }
        [HttpPost]
        public async Task<IActionResult> SubmitRating([FromBody] RatingRequest request ,int Userid)
        {
            var newRating = new Rating
            {
                ArticleId = request.ArticleId,
                UserId = Userid, // 假設你有傳入 UserId
                PositiveRating = request.IsPositive ? 1 : 0,
                NegativeRating = request.IsPositive ? 0 : 1
            };

            _context.Ratings.Add(newRating);
            await _context.SaveChangesAsync();

            // 統計這篇文章所有的正評/負評總和
            int positiveTotal = await _context.Ratings
                .Where(r => r.ArticleId == request.ArticleId)
                .SumAsync(r => r.PositiveRating ?? 0);

            int negativeTotal = await _context.Ratings
                .Where(r => r.ArticleId == request.ArticleId)
                .SumAsync(r => r.NegativeRating ?? 0);

            return Ok(new
            {
                success = true,
                positive = positiveTotal,
                negative = negativeTotal
            });
        }
        public class RatingRequest
        {
            public int ArticleId { get; set; }
            public bool IsPositive { get; set; }
        }
        /// <summary>
        /// 新增留言
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ArticleResponse([FromBody] ArticleResponse request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Comment))
                {
                    return BadRequest("留言內容不能為空白");
                }

                var newResponse = new ArticleResponse
                {
                    ArticleId = request.ArticleId,
                    Comment = request.Comment,
                    UserId = request.UserId,   // 這裡必須傳正確的UserId
                    CreateTime = DateTime.Now
                };

                _context.ArticleResponses.Add(newResponse);
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

}
