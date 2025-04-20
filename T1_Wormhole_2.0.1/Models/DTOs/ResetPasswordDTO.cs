using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using T1_Wormhole_2._0._1.Models;

namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class ResetPasswordDTO
    {
        [StringLength(maximumLength: 20, MinimumLength = 8)]
        /// <summary>
        /// 密碼
        /// </summary>
        public string Password { get; set; } = null!;

        [Required]
        [Compare("Password")]
        [NotMapped]
        public string ConfirmPassword { get; set; }

        public string? Email { get; set; } = null!;

        public string? EmailVerificationToken { get; set; }

    }
}
