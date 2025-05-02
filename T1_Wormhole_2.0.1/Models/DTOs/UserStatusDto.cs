namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class UserStatusDto
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 使用者留言數
        /// </summary>
        public int? CommentCount { get; set; }

        /// <summary>
        /// 使用者閱讀數
        /// </summary>
        public int? ReadCount { get; set; }

        /// <summary>
        /// 使用者發文數
        /// </summary>
        public int? PostCount { get; set; }

        /// <summary>
        /// 使用者是否達瀏覽數上限
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 使用者等級
        /// </summary>
        public int Level { get; set; }
        public string? EmpInfo { get; set; }
    }
}
