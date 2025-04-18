using System.ComponentModel.DataAnnotations;

namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        
        public string? EmailVerificationToken {get; set;}

    }
}
