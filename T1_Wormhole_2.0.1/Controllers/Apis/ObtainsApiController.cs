using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1.Models.Database;

namespace T1_Wormhole_2._0._1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObtainsApiController : ODataController
    {
        private readonly WormHoleContext _context;

        public ObtainsApiController(WormHoleContext context)
        {
            _context = context;
        }

        // GET: api/ObtainsApi/GetObtains
        [EnableQuery]
        [HttpGet]
        
        public async Task<IQueryable<ObtainDTO>> GetObtains()
        {
            return _context.Obtains.Select(e => new ObtainDTO
            {
                ObtainID = e.ObtainId,
                Type = e.Type,
                Name = e.Name,
                Picture =null,
                Price = e.Price,
                Condition = e.Condition,

            });

        }

        // POST: api/ObtainsApi/Filter
        
        [HttpPost("Filter")]
        public async Task<IEnumerable<ObtainDTO>>FilterObtain([FromBody] ObtainDTO obtainDTO)
            {
            return _context.Obtains.Where(e => 
            e.Name.Contains(obtainDTO.Name) ||
            e.Price == obtainDTO.Price ||
            e.Condition.Contains(obtainDTO.Condition))
            .Select(e => new ObtainDTO {
                ObtainID = e.ObtainId,
                Type = e.Type,
                Name = e.Name,
                Picture = null,
                Price = e.Price,
                Condition = e.Condition,
            });
            }

        //GET: api/ObtainsApi/GetPhoto/1
        [HttpGet("GetPicture/{id}")]
        public async Task<FileResult> GetPicture(int id)
        {
            string Filename = Path.Combine("wwwroot", "images", "noimage.jpg");
            Obtain e = await _context.Obtains.FindAsync(id);
            byte[] ImageContent = e?.Picture != null ? e.Picture : System.IO.File.ReadAllBytes(Filename); //e有值取picture 沒有值就取null
                                                                                                      
            return File(ImageContent, "image/jpeg");
        }


        // GET: api/ObtainsApi/GetObtain/5
        [HttpGet("{id}")]
        public async Task<ObtainDTO> GetObtain(int id)
        {
            var obtain = await _context.Obtains.FindAsync(id);

            if (obtain == null)
            {
                return null;
            }
            ObtainDTO obDTO = new ObtainDTO
            {
                ObtainID = obtain.ObtainId,
                Type = obtain.Type,
                Name = obtain.Name,
                Price = obtain.Price,
                Condition = obtain.Condition,
                Picture = null,
            };
            return obDTO;
        }

        // PUT: api/ObtainsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<String> PutObtain( int id,[FromForm] ObtainDTO obtDTO)
        {
            if (id != obtDTO.ObtainID)
            {
                return "修改稱號失敗";
            }

            Obtain obt = await _context.Obtains.FindAsync(id);
            obt.Type = obtDTO.Type;
            obt.Name = obtDTO.Name;
            obt.Price = obtDTO.Price;
            obt.Condition = obtDTO.Condition;
            if (obtDTO.Picture != null)
            {
                using (BinaryReader br = new BinaryReader(obtDTO.Picture.OpenReadStream()))
                {
                    obt.Picture = br.ReadBytes((int)obtDTO.Picture.Length);
                }
            }
            _context.Entry(obt).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObtainExists(id))
                {
                    return "修改稱號失敗";
                }
                else
                {
                    throw;
                }
            }

            return "修改稱號成功";
        }

        // POST: api/ObtainsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<string> PostObtain([FromForm] ObtainDTO obtDTO)
        {
            Obtain obt = new Obtain
            {
                ObtainId = 0,
                Type = obtDTO.Type,
                Name = obtDTO.Name,
                Price = obtDTO.Price,
                Condition = obtDTO.Condition,
             };
            if (obtDTO.Picture != null)
            {
                using (BinaryReader br = new BinaryReader(obtDTO.Picture.OpenReadStream()))
                {
                    obt.Picture = br.ReadBytes((int)obtDTO.Picture.Length);
                }
            }
            _context.Obtains.Add(obt);
            await _context.SaveChangesAsync();
            return $"Obtain編號:{obt.ObtainId}";
        }

        // DELETE: api/ObtainsApi/5
        [HttpDelete("{id}")]
        public async Task<string> DeleteObtain(int id)
        {
            var obtain = await _context.Obtains.FindAsync(id);
            if (obtain == null)
            {
                return "刪除Obtain失敗!";
            }

            _context.Obtains.Remove(obtain);
            await _context.SaveChangesAsync();
            return "刪除Obtain成功!";
        }

        private bool ObtainExists(int id)
        {
            return _context.Obtains.Any(e => e.ObtainId == id);
        }
    }
}
