namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class RatingsDTOs
    {
        public int articlesId { get; set; }
        public int? prCount { get; set; }
        public int? ngrCount { get; set; }
        public int? popularCount { get; set; }

        public string articleTitle { get; set; }

        public string? Name { get; set; }
        public bool Type { get; set; }
    }
}
