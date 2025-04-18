using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1.Models.DTOs;
using T1_Wormhole_2._0._1.Models.Database;

namespace T1_Wormhole_2._0._1.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ArticlesApiController : ControllerBase
    {
        private readonly WormHoleContext _context;

        public ArticlesApiController(WormHoleContext context)
        {
            _context = context;
        }

        // GET: api/ArticlesApi
        [HttpGet]
        
        public async Task<IEnumerable<ArticleDTO>> GetArticles()
        {
           return _context.Articles.Select(e => new ArticleDTO
            {
                ArticleID = e.ArticleId,
                Title = e.Title,
                Type = e.Type,
                CreateTime = e.CreateTime,
                Content = e.Content,
                WriterID = e.WriterId,
                ReleaseBy = e.ReleaseBy,

            });

        }
        public async Task<IEnumerable<ArticleDTO>> GetArticles(int id)
        {
            return _context.Articles.Select(e => new ArticleDTO
            {
                ArticleID = e.ArticleId,
                Title = e.Title,
                Type = e.Type,
                CreateTime = e.CreateTime,
                Content = e.Content,
                WriterID = e.WriterId,
                ReleaseBy = e.ReleaseBy,

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

        // PUT: api/ArticlesApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int id, Article article)
        {
            if (id != article.ArticleId)
            {
                return BadRequest();
            }

            _context.Entry(article).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ArticlesApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Article>> PostArticle(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArticle", new { id = article.ArticleId }, article);
        }

        // DELETE: api/ArticlesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }
    }
}
