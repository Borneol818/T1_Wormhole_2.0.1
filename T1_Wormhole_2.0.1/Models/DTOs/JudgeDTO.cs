namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class JudgeDTO
    {
        public int JudgeID { get; set; }
        public int? Type { get; set; }
        public string Title { get; set; }
        public IFormFile? Picture { get; set; }
        public int? Price { get; set; }
        public int? Rate { get; set; }
        public string? Content { get; set; }
    }
}