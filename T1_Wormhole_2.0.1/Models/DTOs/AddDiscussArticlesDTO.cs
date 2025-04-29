namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class AddDiscussArticlesDTO
    {
        public string Title { get; set; }
        public bool Type { get; set; } 
        public string Content { get; set; } 
        public DateTime CreateTime{ get; set; }

        public string[]? Signature { get; set; }

        public IFormFile? ArticleCover { get; set; }
    }
}
