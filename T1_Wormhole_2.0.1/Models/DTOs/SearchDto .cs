namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class SearchDTO
    {        
        public int? ResultUserId { get; set; }
        public int? ResultArticleId { get; set; }
        //public int? ArticleWriterId { get; set; }
        public string? ArticleTitle { get; set; }
        public string? ArticleContent { get; set; }
        public IFormFile? ArticleCover { get; set; }
        public string? UserName { get; set; }
        public string? UserNickname { get; set; }
        public IFormFile? UserPhoto { get; set; }
        public string? UserSignature { get; set; }
        




    }
}