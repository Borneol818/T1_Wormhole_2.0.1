namespace T1_Wormhole_2._0._1.Models.DTOs
{ 
    public class ArticleDTO
    {
        public int ArticleID { get; set; }
        public string Title { get; set; }
        public bool Type { get; set; }
        public DateTime CreateTime { get; set; }
        public string FormattedCreateTime => CreateTime.ToString("yyyy/MM/dd HH:mm");
        public string? Content { get; set; }
        public int? WriterID { get; set; }
        public int? ReleaseBy { get; set;}

        public IFormFile? ArticleCover { get; set; }
        public string WriterNickname { get; set; }
        public string ReleaseByName { get; set; }
        public int CommentCount { get; set; }


    }
}