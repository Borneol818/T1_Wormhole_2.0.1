namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class RelationDto
    {
        public string RelationType { get; set; }

        public int InviterId { get; set; }

        public int InviteeId { get; set; }

        public string Invite { get; set; }
        public string InviterName { get; set; }
        public string InviteeName { get; set; }
    }
}
