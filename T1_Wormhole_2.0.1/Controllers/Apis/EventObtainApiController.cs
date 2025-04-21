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

        //post:/api/EventObtain/joinEvent
        [HttpPost]
        public async Task<String> joinEvent(ParticipationDTO participationDTO) {
            //複合主鍵必須傳入所有主鍵值進行搜尋，參數順序依照主鍵的定義順序填入
            var joinData = await _db.Participations.FindAsync(participationDTO.EventId,participationDTO.UserId);
            if (joinData != null)
            {
                return "活動已有參與資料";
            }
            Participation p=new Participation
            {
                EventId=participationDTO.EventId,
                UserId=participationDTO.UserId,
                JoinTime=DateTime.Now,
                Status="參加中",
            };
            _db.Participations.Add(p);
            await _db.SaveChangesAsync();
            return "成功參與活動";
        }
    }
}
