using System.ComponentModel.DataAnnotations;
using T1_Wormhole_2._0._1.Models.Database;

namespace T1_Wormhole_2._0._1.LoginScripts
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Username or Email is required")]
        public string UserIdentifier {  get; set; }


        [Required]
        public string Password { get; set; }

        public string? KeepLog { get; set; }

        public string? RememberMe { get; set; }
    }
}
