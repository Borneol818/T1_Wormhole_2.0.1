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
        public async Task<IActionResult> SubmitRating([FromBody] RatingRequest request)
        {
            // 查找是否有該文章的評價記錄
            var rating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.ArticleId == request.ArticleId);

            if (rating == null)
            {
                // 如果沒有記錄，新增一筆
                rating = new Rating
                {
                    ArticleId = request.ArticleId,
                    PositiveRating = request.IsPositive ? 1 : 0,
                    NegativeRating = request.IsPositive ? 0 : 1
                };
                _context.Ratings.Add(rating);
            }
            else
            {
                //更新喜歡或不喜歡
                if (request.IsPositive)
                {
                    rating.PositiveRating = (rating.PositiveRating ?? 0) + 1;
                }
                else
                {
                    rating.NegativeRating = (rating.NegativeRating ?? 0) + 1;
                }
            }

            // 儲存變更到資料庫
            await _context.SaveChangesAsync();

            // 回傳成功訊息
            return Ok(new { success = true });

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
