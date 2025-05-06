namespace T1_Wormhole_2._0._1.Models.Database
{
    public partial class LoginRecord
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime Time { get; set; }

        public virtual UserInfo User { get; set; } = null!;
    }
}
