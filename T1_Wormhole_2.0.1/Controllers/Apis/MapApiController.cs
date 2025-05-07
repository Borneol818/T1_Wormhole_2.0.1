using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;

namespace T1_Wormhole_2._0._1.Controllers.Apis
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapApiController : ControllerBase
    {
        private readonly WormHoleContext _context;

        public MapApiController(WormHoleContext context)
        {
            _context = context;
        }

        [HttpPost("UpdateLocation")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationDTO request)
        {
            if (request == null || request.Position == null || request.Position.Lat == 0 || request.Position.Lng == 0)
            {
                return BadRequest(new { message = "Invalid request payload" });
            }

            var user = await _context.UserInfos.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            try
            {
                var point = new Point(request.Position.Lng, request.Position.Lat) { SRID = 4326 }; // WGS84 coordinate system
                user.Position = point;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Location updated successfully" });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "Error updating location", error = ex.Message });
            }
        }

        [HttpGet("GetFriends")]
        public async Task<IActionResult> GetFriends()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            int userId;
            try
            {
                userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
            }
            catch (Exception)
            {
                return Unauthorized(new { message = "Invalid user ID" });
            }

            var friends = await _context.Relations
                .Where(r => (r.InviterId == userId || r.InviteeId == userId) &&
                           r.RelationType == "Friend" &&
                           r.Invite == "已接受")
                .SelectMany(r => new[] { r.InviteeId, r.InviterId }
                    .Where(id => id != userId))
                .Select(id => new
                {
                    UserId = id,
                    UserInfo = _context.UserInfos
                        .Where(u => u.UserId == id)
                        .Select(u => new
                        {
                            u.UserId,
                            u.Nickname,
                            Position = u.Position != null? u.Position.ToString(): null
                        })
                        .FirstOrDefault()
                })
                .Where(x => x.UserInfo != null)
                .Select(x => x.UserInfo)
                .ToListAsync();

            if (friends == null || !friends.Any())
            {
                return NotFound(new { message = "No friends found" });
            }

            return Ok(friends);
        }
    }
}
