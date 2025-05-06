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
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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

            return _context.Articles
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
                    WriterID = e.WriterId,
                    WriterNickname = e.Writer.Nickname,
                    ReleaseByName = e.ReleaseByNavigation.Name,
                    CommentCount = e.ArticleResponses.Count(),
                    ArticleCover = null
                })
            .OrderByDescending(e => e.CreateTime)//依照文章時間排序新的在前面
            .Take(1000);//最多取1000筆資料
        }

        // GET: api/ArticlesApi
        [HttpGet("News")]
        [EnableQuery]
        public IQueryable<ArticleDTO> GetNewsArticles()
        {

            return _context.Articles
                .Include(a => a.Writer)
                .Include(a => a.ReleaseByNavigation)
                .Include(a => a.ArticleResponses)
                .Where(a => a.Type == false)
                .Select(e => new ArticleDTO
                {
                    ArticleID = e.ArticleId,
                    Title = e.Title,
                    Type = e.Type,
                    CreateTime = e.CreateTime,
                    Content = e.Content,
                    ReleaseByName = e.ReleaseByNavigation.Name,
                    CommentCount = e.ArticleResponses.Count(),
                    ArticleCover = null
                })
            .OrderByDescending(e => e.CreateTime)//依照文章時間排序新的在前面
            .Take(1000);//最多取1000筆資料
        }

        // GET: api/ArticlesApi
        [HttpGet("Discuss")]
        [EnableQuery]
        public IQueryable<ArticleDTO> GetDiscussArticles()
        {

            return _context.Articles
                .Include(a => a.Writer)
                .Include(a => a.ReleaseByNavigation)
                .Include(a => a.ArticleResponses)
                .Where(a => a.Type == true)
                .Select(e => new ArticleDTO
                {
                    ArticleID = e.ArticleId,
                    Title = e.Title,
                    Type = e.Type,
                    CreateTime = e.CreateTime,
                    Content = e.Content,
                    WriterID = e.WriterId,
                    WriterNickname = e.Writer.Nickname,
                    CommentCount = e.ArticleResponses.Count(),
                    ArticleCover = null
                })
            .OrderByDescending(e => e.CreateTime)//依照文章時間排序新的在前面
            .Take(1000);//最多取1000筆資料
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

        //GET: api/ArticlesApi/GetPicture/1
        [HttpGet("GetPicture/{id}")]
        public async Task<FileResult> GetPicture(int id)
        {
            string Filename = Path.Combine("wwwroot", "images", "noimages.jpg");
            Article e = await _context.Articles.FindAsync(id);
            byte[] ImageContent = e?.ArticleCover != null ? e.ArticleCover : System.IO.File.ReadAllBytes(Filename); //e有值取ArticleCover 沒有值就取null

            return File(ImageContent, "image/jpeg");
        }


        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }

        //GET: api/ArticlesApi/isUser
        [HttpGet("isUser")]
        public bool isUser()
        {
            if (User.FindFirst(ClaimTypes.Role)?.Value == "User")
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        //GET: api/ArticlesApi/isUser
        [HttpGet("isAdmin")]
        public bool isAdmin()
        {
            if (User.FindFirst(ClaimTypes.Role)?.Value == "Admin")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
