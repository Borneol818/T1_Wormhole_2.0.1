using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;

namespace T1_Wormhole_2._0._1.Controllers.Apis
{
    [Route("api/[controller]")]
    [ApiController]
    public class JudgeApiController : ODataController
    {
        private readonly WormHoleContext _context;

        public JudgeApiController(WormHoleContext context)
        {
            _context = context;
        }

        // GET: api/JudgeApi
        [EnableQuery]
        [HttpGet]

        public async Task<IQueryable<JudgeDTO>> GetJudges()
        {
            return _context.Judges.Select(e => new JudgeDTO
            {
                JudgeID = e.JudgeId,
                Type = e.Type,
                Title = e.Title,
                Picture = null,
                Price = e.Price,
                Rate = e.Rate,
                Content = e.Content,

            });

        }

        //GET: api/JudgeApi/GetPicture/1
        [HttpGet("GetPicture/{id}")]
        public async Task<FileResult> GetPicture(int id)
        {
            string Filename = Path.Combine("wwwroot", "images", "noimage.jpg");
            Judge e = await _context.Judges.FindAsync(id);
            byte[] ImageContent = e?.Picture != null ? e.Picture : System.IO.File.ReadAllBytes(Filename); //e有值取picture 沒有值就取null

            return File(ImageContent, "image/jpeg");
        }

        // POST: api/JudgeApi/Filter

        [HttpPost("Filter")]
        public async Task<IEnumerable<JudgeDTO>> FilterJudge([FromBody] JudgeDTO JudgeDTO)
        {
            return _context.Judges.Where(e =>
            e.Title.Contains(JudgeDTO.Title) ||
            e.Price == JudgeDTO.Price ||
            e.Type == JudgeDTO.Type||
            e.Content.Contains(JudgeDTO.Content))
            .Select(e => new JudgeDTO
            {
                JudgeID = e.JudgeId,
                Type = e.Type,
                Title = e.Title,
                Picture = null,
                Price = e.Price,
                Rate = e.Rate,
                Content = e.Content,
            });
        }

        private bool JudgeExists(int id)
        {
            return _context.Judges.Any(e => e.JudgeId == id);
        }
    }
}
