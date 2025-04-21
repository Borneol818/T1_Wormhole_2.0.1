using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;

namespace T1_Wormhole_2._0._1.Controllers.Apis
{
    [Route("api/EventObtain/[action]")]
    [ApiController]
    public class EventObtainApiController : ControllerBase
    {
        private readonly WormHoleContext _db;

        public EventObtainApiController(WormHoleContext db)
        {
            _db = db;
        }

        //get:/api/EventObtain/getAllEvent
        [HttpGet]
        public async Task<IEnumerable<EventDTO>> getAllEvent() 
        {
            return await _db.Events.Select(e => new EventDTO
            {
                Title = e.Title,
                EventContent=e.EventContent,
                CreateTime = e.CreateTime,
                EventId = e.EventId,
                Marquee=e.Marquee,
                ManagerId=e.ManagerId,
                Coin=e.Coin,
                Obtain=e.Obtain,
                EventTimeStrat=e.EventTimeStrat,
                EventTimeEnd=e.EventTimeEnd,
                Type=e.Type, 
            }).ToListAsync();
        }
    }
}
