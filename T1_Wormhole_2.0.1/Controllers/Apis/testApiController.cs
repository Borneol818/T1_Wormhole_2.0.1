using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;

namespace T1_Wormhole_2._0._1.Controllers.Apis
{
    [Route("api/test/[action]")]
    [ApiController]
    public class testApiController : ControllerBase
    {
        private readonly WormHoleContext _db;

        public testApiController(WormHoleContext db)
        {
            _db = db;
        }

        //get:/api/test/getAllEvent
        [HttpGet]
        public bool Delay()
        {
            BackgroundJob.Schedule(() => this.DoSomething(), TimeSpan.FromSeconds(20));
            return true;
        }



        [NonAction]
        public void DoSomething() 
        {
            _db.Events.Add(new Event
            {
                Coin = 1,
                CreateTime = DateTime.Now,
                EventContent = "Hello",
                EventTimeEnd = DateTime.Now.AddDays(1),
                EventTimeStrat = DateTime.Now,
                Marquee = "123",
                ManagerId = 1,
                Title = "Title",
                Type = "Type",
            });
            _db.SaveChanges();
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

        public void giveReward() 
        {
            //使用者點選參加活動之後登陸到資料庫(v)
            //達成獎勵條件發放獎勵
            
            //有留言活動、貼文活動，依照活動不同判斷邏輯不同
            //用type分活動執行類型、執行函式

            //活動頁生成專屬連結，點連結進去後的po文或留言才算獎勵
            //or特定文章有在資料庫紀錄參與的活動，使用者貼文可以選擇參與哪些活動

            //判斷點
            //1.在活動頁有一個領取獎勵的按鈕，點選之後會依照使用者留言、發文紀錄，確認是否符合資格
            //(可能漏領獎勵)
            //2.在每次發文、留言時都進入活動獎勵判斷，如果符合資格就及時發放
            //(如果未來獎勵活動類型一多，判斷觸發點也要重做；另外萬一觸發獎勵發放發生bug，剛好獎勵沒發到，那也無法補領)
        }
    }
}
