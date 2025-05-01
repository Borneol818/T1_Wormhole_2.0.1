namespace T1_Wormhole_2._0._1.Models
{
    public class RatingRequestDTO
    {
        public int ArticleId { get; set; }    // 文章 ID
        public int UserId { get; set; }       // 使用者 ID
        public bool IsPositive { get; set; }  // 是否為喜歡的評價 (True = 喜歡, False = 不喜歡)
    }
}
