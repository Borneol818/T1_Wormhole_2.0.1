namespace T1_Wormhole_2._0._1.Models.DTOs
{ 
    public class CommentDto
    {
        public int? ArticleID { get; set; }
        public string? Title { get; set; }        
        //public DateTime? CreateTime { get; set; }
        public int? UserID { get; set; }       
        public string? Comment { get; set; }
        public int? Id { get; set; }
    }
}