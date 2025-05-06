using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace T1_Wormhole_2._0._1.Models.Database
{
    public partial class LoginRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime Time { get; set; }

        public virtual UserInfo User { get; set; } = null!;
    }
}
