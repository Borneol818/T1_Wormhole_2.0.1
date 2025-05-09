using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;
using Hangfire;
using System.Security.Claims;

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
                EventTimeStrat=e.EventTimeStrat.ToString("yyyy年MM月dd日 HH點mm分"),
                EventTimeEnd=e.EventTimeEnd.ToString("yyyy年MM月dd日 HH點mm分"),
                Type=e.Type, 

            }).ToListAsync();
        }

        //get:/api/EventObtain/getUser
        [HttpGet]
        public async Task<int> getUser() 
        {
            return Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
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

        //get:/api/EventObtain/Test?eId=&aId=
        [HttpGet]
        public async Task<Event> Test(int eId,int aId) 
        {
            var a=await _db.Articles.FindAsync(aId);
            var e = await _db.Events.FindAsync(eId);
            if (a.CreateTime < e.EventTimeEnd)
            {
                return e;
            }
            else { return null; }
        }

        //--------hangFire活動獎勵發放定時---------
        //定時時間:每日0:00:00執行
        [HttpGet]
        //get:/api/EventObtain/DistributeEventRewards
        public bool DistributeEventRewards() {
            RecurringJob.AddOrUpdate("GiveEventReward", ()=>this.findExpiredEvents(),Cron.Daily());
            
            //測試用
            //注意:定時活動會永久存在，不用每次打開都執行一次(再次執行同名會覆蓋)
            //RecurringJob.AddOrUpdate("GiveEventReward", () => this.findExpiredEvents(), Cron.Minutely());
            return true;
        }
        //--------hangFire活動獎勵發放定時---------
        //--------活動獎勵定時觸發用函式----------
        [NonAction]
        public async Task findExpiredEvents() 
        {
            //篩選已經到期，且到期一天內的活動(每天定時篩選的話，就會是篩前一天沒篩到的活動)
            var events = _db.Events.Where(e=>e.EventTimeEnd<=DateTime.Now&& e.EventTimeEnd.AddDays(1)>=DateTime.Now).Select(e=>e.EventId).ToList();
            if (events.Count < 1) return;
            foreach (var e in events) 
            {
              await GiveEventReward(e);
            }
        }
        //--------活動獎勵定時觸發用函式----------

        //--------活動獎勵發放(hangFire定時)---------
        //一次篩選所有參與活動的使用者是否符合發放資格

        //測試用get:/api/EventObtain/GiveEventReward?eventId=
        [NonAction]
        public async Task<string> GiveEventReward(int eventId) 
        {
            //---達成獎勵條件發放獎勵---

            //判斷點
            //利用hangFire在活動結束後發送函式，依照符合資格者發放獎勵
            //定時函式>把資料表所有event抓出來，抓到到期活動把id丟來這邊跑

            //一次開資料庫拿到所有這個活動的資料(後續不用再開資料庫，不用非同步，可是資料會佔記憶體空間)
            var EventJoinData = await _db.Participations.Where(p => p.EventId == eventId).ToListAsync();
            var EventData =await _db.Events.Where(e=>e.EventId==eventId).ToListAsync();
            if (EventJoinData.Count == 0 || EventJoinData == null) return null;

            //拿到對應活動的參與者
            //(後續優化選項:再把Participation跟Article join再查詢)
            var joinEventUsersID =EventJoinData.Select(p=>p.UserId);
            var eventStartTime = EventData.Where(e => e.EventId == eventId).Select(e=>e.EventTimeStrat);
            
            //空陣列裝可發放獎勵的user用
            List<int?> rewardUsers=new List<int?>();
            //用文章類型過濾使用函式(用switch未來可擴充其他事件)
            switch (EventData.First().Type) 
            {
                //文章資料多，一次過濾完
                //過濾有報名活動、在報名之後po文、且文章標題含有keyword的使用者(符合獎勵資格)            
                //註:字串contains與陣列contains用法略不同，字串有「包含」即會回傳true，集合是精確比對，完全相等才會回傳true
                case "徵文活動":
                    rewardUsers = await _db.Articles
                        .Where(a => joinEventUsersID.Contains((int)a.WriterId) && a.CreateTime > eventStartTime.First())//理論上會進到篩選代表活動剛結束，不特別再篩結束時間
                        .Where(a => a.Title.Contains(EventData.First().KeyWord))
                        .Select(a => a.WriterId).Distinct().ToListAsync();
                    break;
                //留言一樣做特定時間內留言的關鍵字篩選
                //*不一樣在留言需要達到特定數量
                case "留言活動":
                    rewardUsers = await _db.ArticleResponses
                        .Where(ar => joinEventUsersID.Contains(ar.UserId) && ar.CreateTime > eventStartTime.First())
                        .Where(ar => ar.Comment.Contains(EventData.First().KeyWord))
                        .GroupBy(ar=>ar.UserId).Where(g=>g.Count()>=EventData.First().EventTarget)
                        .Select(ar=>(int?)ar.Key).Distinct().ToListAsync();
                    break ;
                default:
                    break;
            }
            if (rewardUsers.Count < 1) return null;

            if (EventData.First().Coin!=null) {
                //發放獎勵，在forumCoins新增紀錄
                //*userStatus wallet可能也要更新
                foreach (var a in rewardUsers)
                {
                    //如果有此user的活動發放紀錄就不加入，進入下一個迴圈
                    if (_db.ForumCoins.Any(f => f.UserId == a.Value && f.CoinSource == EventData.First().Title)) continue;
                    _db.ForumCoins.Add(new ForumCoin
                    {
                        UserId = a.Value,
                        CoinSource = EventData.First().Title,
                        AccessTime = DateTime.Now,
                        CoinAmount = EventData.First().Coin.Value,
                        Status = "已發放"
                    });
                }
            }

            if (EventData.First().Obtain != null)
            {
                foreach (var a in rewardUsers)
                {
                    //如果有此user的活動發放紀錄就不加入，進入下一個迴圈
                    if (_db.ObtainStatuses.Any(f => f.UserId == a.Value && f.ObtainId == EventData.First().Obtain)) continue;
                    _db.ObtainStatuses.Add(new ObtainStatus
                    {
                        UserId = a.Value,
                        ObtainId = (int)EventData.First().Obtain,
                        Time = DateTime.Now,
                        Count = 1,
                        Status = "使用中"
                    });
                }
            }
            

            await _db.SaveChangesAsync();

            return "完成獎勵發放";
        }
        //--------活動獎勵發放(hangFire定時)---------
    }
}
