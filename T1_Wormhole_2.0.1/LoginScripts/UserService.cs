using T1_Wormhole_2._0._1.Models.Database;

namespace T1_Wormhole_2._0._1.LoginScripts
{
    public interface IUserService
    {
        Login CreateUser(Login user);
        Login FindByUsername(string username);
        Login FindByEmail(string email);
        void UpdateUser(Login user);

        Task LoginRewardAsync(int userId);
    }

    
    public class UserService : IUserService
    {
        private readonly WormHoleContext _context;

        public UserService(WormHoleContext context)
        {
            _context = context;
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

        public async Task LoginRewardAsync(int userId)
        {
            var LoginStatus = _context.UserStatuses.FirstOrDefault(u => u.Id == userId).Logintoday;
            if (!LoginStatus)
            {
                var LoginCoin = new ForumCoin
                {
                    CoinId = 0,
                    UserId = userId,
                    CoinSource = "每日登入活動",
                    AccessTime = DateTime.Now,
                    CoinAmount = 2,
                    Status = "已發放"
                };

                _context.ForumCoins.Add(LoginCoin);
                await _context.SaveChangesAsync();
            }

        }
    }
}
