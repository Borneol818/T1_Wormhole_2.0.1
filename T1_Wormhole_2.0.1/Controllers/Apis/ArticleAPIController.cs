using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T1_Wormhole_2._0._1.Models.Database;
using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using T1_Wormhole_2._0._1.Models.DTOs;
using System.Security.Claims;

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
                date = r.CreateTime.ToString("yyyy年MM月dd日 HH點mm分")

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
        public async Task<IActionResult> SubmitRating([FromBody] RatingRequestDTO request)
        {
            int UserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (request.ArticleId <= 0 || UserId <= 0)
            {
                return BadRequest("參數錯誤");
            }

            try
            {
                // 嘗試查找使用者是否已經對這篇文章評價過
                var rating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.ArticleId == request.ArticleId && r.UserId == UserId);

                if (rating == null)
                {
                    // 如果使用者沒有評價過，新增一筆新的評價資料
                    var newRating = new Rating
                    {
                        ArticleId = request.ArticleId,
                        UserId = UserId,
                        PositiveRating = request.IsPositive ? 1 : 0,
                        NegativeRating = request.IsPositive ? 0 : 1
                    };
                    _context.Ratings.Add(newRating);
                }
                else
                {
                    // 如果使用者已經評價過，則更新其評價

                    rating.PositiveRating = request.IsPositive ? 1 : 0;
                    rating.NegativeRating = request.IsPositive ? 0 : 1;
                    _context.Ratings.Update(rating);
                }

                await _context.SaveChangesAsync();

                // 計算更新後的統計資料
                var posCount = await _context.Ratings
                    .Where(r => r.ArticleId == request.ArticleId)
                    .SumAsync(r => r.PositiveRating);

                var negCount = await _context.Ratings
                    .Where(r => r.ArticleId == request.ArticleId)
                    .SumAsync(r => r.NegativeRating);

                var article = await _context.Articles
                    .Include(a => a.Writer) // 確保載入作者資訊
                    .FirstOrDefaultAsync(a => a.ArticleId == request.ArticleId);

                var dto = new RatingsDTOs
                {
                    articlesId = request.ArticleId,
                    prCount = posCount,
                    ngrCount = negCount,
                    popularCount = (posCount ?? 0) + (negCount ?? 0),
                    articleTitle = article?.Title ?? "未知標題",
                    Name = article?.Writer?.Nickname ?? "匿名"
                };

                return Ok(new { success = true, data = dto });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("Error: " + ex.InnerException?.Message);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
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
