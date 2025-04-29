using System.ComponentModel.DataAnnotations;

namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class BOManagerLoginDTO
    {
        [Required]
        public string Account { get; set; }

        [Required] 
        public string Password { get; set; }
    }
}
