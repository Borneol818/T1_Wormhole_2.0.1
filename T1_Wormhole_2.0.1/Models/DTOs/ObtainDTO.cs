namespace T1_Wormhole_2._0._1.Controllers
{
    public class ObtainDTO
    {
        public int ObtainID { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }        
        public IFormFile? Picture { get; set; }
        public int Price { get; set; }
        public string Condition { get; set; }
        public string? ShowPicture { get; set; }
        public int? ItemType { get; set; }

    }
}