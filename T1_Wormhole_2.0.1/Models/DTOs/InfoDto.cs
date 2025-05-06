namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class InfoDto
    {
        public string? ManagerName { get; set; }
        public string? ManagerTeam { get; set; }
        public int? UserId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Nickname { get; set; }

        public DateOnly? Birthday { get; set; }

        public IFormFile? Photo { get; set; }

        public string? Signature { get; set; }

        public bool? Sex { get; set; }
        public int? Wallet { get; set; }
        public string? EmpInfo { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? IsLogin { get; set; }
    }
}
