using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1.Models.Database;

namespace T1_Wormhole_2._0._1.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ObtainsApiController : ControllerBase
    {
        private readonly WormHoleContext _context;

        public ObtainsApiController(WormHoleContext context)
        {
            _context = context;
        }

        // GET: api/ObtainsApi
        [HttpGet]
        
        public async Task<IEnumerable<ObtainDTO>> GetObtains()
        {
            return _context.Obtains.Select(e => new ObtainDTO
            {
                ObtainID = e.ObtainId,
                Type = e.Type,
                Name = e.Name,
                Picture = e.Picture,
                Price = e.Price,
                Condition = e.Condition,

            });

        }

        // GET: api/ObtainsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Obtain>> GetObtain(int id)
        {
            var obtain = await _context.Obtains.FindAsync(id);

            if (obtain == null)
            {
                return NotFound();
            }

            return obtain;
        }

        // PUT: api/ObtainsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutObtain(int id, Obtain obtain)
        {
            if (id != obtain.ObtainId)
            {
                return BadRequest();
            }

            _context.Entry(obtain).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObtainExists(id))
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

        // POST: api/ObtainsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Obtain>> PostObtain(Obtain obtain)
        {
            _context.Obtains.Add(obtain);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetObtain", new { id = obtain.ObtainId }, obtain);
        }

        // DELETE: api/ObtainsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObtain(int id)
        {
            var obtain = await _context.Obtains.FindAsync(id);
            if (obtain == null)
            {
                return NotFound();
            }

            _context.Obtains.Remove(obtain);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ObtainExists(int id)
        {
            return _context.Obtains.Any(e => e.ObtainId == id);
        }
    }
}
