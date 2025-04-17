namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class UserInfoDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string? Nickname { get; set; }
        
        public string Birthday { get; set; }

        public IFormFile? Photo { get; set; }

        public string? Signature { get; set; }

        public bool Sex { get; set; }
        public int? Wallet { get; set; }

    }
}