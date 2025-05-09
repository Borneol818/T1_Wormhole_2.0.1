using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1.Models.Database;

namespace T1_Wormhole_2._0._1.LoginScripts
{
    public interface IUserService
    {
        Login CreateUser(Login user);
        Login FindByUsername(string username);
        Login FindByEmail(string email);
        void UpdateUser(Login user);

        Task <bool> DailyRewardAsync(int userId);
    }


    public class UserService : IUserService
    {
        private readonly WormHoleContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(WormHoleContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Login CreateUser(Login user)
        {
            _context.Logins.Add(user);
            _context.SaveChanges();
            return user;
        }


        public Login FindByUsername(string username)
        {
            return _context.Logins.FirstOrDefault(u => u.Account == username);
        }

        public Login FindByEmail(string email)
        {
            return _context.Logins.FirstOrDefault(u => u.Email == email);
        }

        public void UpdateUser(Login user)
        {
            _context.Logins.Update(user);
            _context.SaveChanges();
        }


        public async Task<bool> DailyRewardAsync(int userId)
        {
            try
            {
                var taiwanZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, taiwanZone);
                var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Unspecified);

                // 檢查當天是否已有記錄
                var hasVisitToday = await _context.LoginRecords.AnyAsync(l => l.UserId == userId && l.Time >= todayStart);


                if (!hasVisitToday)
                {
                    var user = await _context.UserInfos.FindAsync(userId);
                    if (user != null)
                    {
                        var LoginAward = new ForumCoin
                        {
                            CoinId = 0,
                            UserId = userId,
                            CoinSource = "每日登入獎勵",
                            CoinAmount = 2,
                            AccessTime = DateTime.UtcNow.AddHours(8),
                            Status = "已發放"
                        };
                        var LoginRecord = new LoginRecord
                        {
                            UserId = user.UserId,
                            Time = DateTime.UtcNow.AddHours(8),
                        };
                        _context.ForumCoins.Add(LoginAward);
                        _context.LoginRecords.Add(LoginRecord);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發放每日獎勵失敗，UserId: {UserId}", userId);
                throw;
            }
        }
    }
}
