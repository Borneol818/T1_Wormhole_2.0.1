

namespace T1_Wormhole_2._0._1.Models.DTOs;

public class EventDTO
{
    /// <summary>
    /// 活動標題
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 活動內容
    /// </summary>
    public string EventContent { get; set; }

    /// <summary>
    /// 活動建立時間
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 活動ID
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 活動跑馬燈
    /// </summary>
    public string Marquee { get; set; }

    /// <summary>
    /// 管理者ID
    /// </summary>
    public int ManagerId { get; set; }

    /// <summary>
    /// 活動發放貨幣
    /// </summary>
    public int? Coin { get; set; }

    /// <summary>
    /// 活動發放稱號
    /// </summary>
    public int? Obtain { get; set; }

    /// <summary>
    /// 活動開始時間
    /// </summary>
    public string EventTimeStrat { get; set; }

    /// <summary>
    /// 活動結束時間
    /// </summary>
    public string EventTimeEnd { get; set; }

    /// <summary>
    /// 活動類別
    /// </summary>
    public string Type { get; set; }

}