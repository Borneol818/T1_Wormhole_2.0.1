using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1.Models.DTOs;
using T1_Wormhole_2._0._1.Models.Database;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;

namespace T1_Wormhole_2._0._1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesApiController : ODataController
    {
        private readonly WormHoleContext _context;

        public ArticlesApiController(WormHoleContext context)
        {
            _context = context;
        }

        // GET: api/ArticlesApi
        [HttpGet]
        [EnableQuery]
        public IQueryable<ArticleDTO> GetArticles()
        {
            
            return  _context.Articles
                .Include(a => a.Writer)
                .Include(a => a.ReleaseByNavigation)
                .Include(a => a.ArticleResponses)
                .Select(e => new ArticleDTO
                {
                ArticleID = e.ArticleId,
                Title = e.Title,
                Type = e.Type,
                CreateTime = e.CreateTime,
                Content = e.Content,
                WriterNickname = e.Writer.Nickname,
                ReleaseByName = e.ReleaseByNavigation.Name,
                CommentCount= e.ArticleResponses.Count(),
                ArticleCover=null
                });

        }
        public IQueryable<ArticleDTO> GetArticles(int id)
        {
            return _context.Articles.Select(e => new ArticleDTO
            {
                ArticleID = e.ArticleId,
                Title = e.Title,
                Type = e.Type,
                CreateTime = e.CreateTime,
                Content = e.Content,
                WriterNickname = e.Writer.Nickname,
                ReleaseByName = e.ReleaseByNavigation.Name,
                ArticleCover = null
            });

        }


        // GET: api/ArticlesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Article>> GetArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            return article;
        }

        //GET: api/ArticlesApi/GetPhoto/1
        [HttpGet("GetPicture/{id}")]
        public async Task<FileResult> GetPicture(int id)
        {
            string Filename = Path.Combine("wwwroot", "images", "noimages.jpg");
            Article e = await _context.Articles.FindAsync(id);
            byte[] ImageContent = e?.Picture != null ? e.Picture : System.IO.File.ReadAllBytes(Filename); //e有值取picture 沒有值就取null

            return File(ImageContent, "image/jpeg");
        }


        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }
    }
}
