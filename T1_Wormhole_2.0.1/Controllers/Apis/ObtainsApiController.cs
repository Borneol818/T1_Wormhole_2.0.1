using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;
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

        // GET: api/ObtainsApi
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
        public async Task<IEnumerable<ObtainDTO>>FilterObtain([FromBody]ObtainDTO obtainDTO)
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

        //GET: api/ObtainsApi/GetPicture/1
        [HttpGet("GetPicture/{id}")]
        public async Task<FileResult> GetPicture(int id)
        {
            string Filename = Path.Combine("wwwroot", "images", "noimage.jpg");
            Obtain e = await _context.Obtains.FindAsync(id);
            byte[] ImageContent = e?.Picture != null ? e.Picture : System.IO.File.ReadAllBytes(Filename); //e有值取picture 沒有值就取null
                                                                                                      
            return File(ImageContent, "image/jpeg");
        }


        // GET: api/ObtainsApi/GetObtain/5
        [Authorize]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
            return $"稱號編號:{obt.ObtainId}";
        }

        // POST: api/ObtainsApi/3
        [Authorize(Roles = "User")]
        [HttpPost("{id}")]
        public async Task<IActionResult> Buy(int id)
        {
          try
          {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserID");
            if (userIdClaim == null)
                return Unauthorized(new { message = "無法取得使用者資訊" });
            int userID = int.Parse(userIdClaim.Value);

            var user = await _context.UserInfos.FindAsync(userID);
            if (user == null)
                return NotFound(new { message = "使用者不存在" });

            var obtain = await _context.Obtains.FindAsync(id);
            if (obtain == null)
                return NotFound(new { message = "稱號不存在" });

            if (obtain.Price == 0)
                return BadRequest(new { message = "此稱號無法購買" });

            // 取得目前總餘額
            var wallet = _context.ForumCoins
                .Where(x => x.UserId == userID && x.Status == "已發放")
                .Sum(x => (int?)x.CoinAmount) ?? 0;

            bool alreadyOwned = _context.ObtainStatuses.Any(x => x.UserId == userID && x.ObtainId == id);
            if (alreadyOwned)
                return BadRequest(new { message = "您已擁有此稱號" });

            if (user.Wallet < obtain.Price)
            return BadRequest(new { message = "餘額不足" });

            
            //寫入稱號狀態表
            var status = new ObtainStatus
            {
                UserId = userID,
                ObtainId = id,
                Time = DateTime.Now,
                Count = 1,
                Status = "使用中"
            };
            _context.ObtainStatuses.Add(status);

            //寫入扣款紀錄
            var coinRecord = new ForumCoin
            {
                UserId = userID,
                CoinSource = $"購買稱號：{obtain.Name}", 
                AccessTime = DateTime.Now,
                CoinAmount = -obtain.Price,
                Status = "已發放"
            };
            _context.ForumCoins.Add(coinRecord);

            // 再次更新 Wallet
            var totalCoins = _context.ForumCoins
                .Where(x => x.UserId == userID && x.Status == "已發放")
                .Sum(x => (int?)x.CoinAmount) ?? 0;

            user.Wallet = totalCoins;

            await _context.SaveChangesAsync();
            return Ok(new { message = "購買成功" });
          }
            catch (Exception ex)
            {
                // log exception
                return StatusCode(500, "伺服器錯誤：" + ex.Message);
            }
        }

        // DELETE: api/ObtainsApi/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<string> DeleteObtain(int id)
        {
            var obtain = await _context.Obtains.FindAsync(id);
            if (obtain == null)
            {
                return "刪除稱號失敗!";
            }

            _context.Obtains.Remove(obtain);
            await _context.SaveChangesAsync();
            return "刪除稱號成功!";
        }

        [HttpGet("isAdmin")]
        public bool isAdmin()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Admin")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpGet("isUser")]
        public bool isUser()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "User")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ObtainExists(int id)
        {
            return _context.Obtains.Any(e => e.ObtainId == id);
        }
    }
}
