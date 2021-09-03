using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.Model
{
    [Table("campaign_schedule")]
    public class CampaignData
    {
        [Column("id", TypeName = "INTEGER"), Key]
        public int CampaignID { get; set; }

        [Column("campaign_category", TypeName = "INTEGER")]
        public int CampaignCategory { get; set; }

        /// <summary>
        /// 倍率×1000.
        /// 例:2倍 -> 2000.0
        /// </summary>
        [Column("value", TypeName = "REAL")]
        public float CampaignValue { get; set; }

        [Column("system_id", TypeName = "INTEGER")]
        public int SystemID { get; set; }

        [Column("icon_image", TypeName = "INTEGER")]
        public int IconImage { get; set; }

        [Column("start_time", TypeName = "INTEGER")]
        public DateTime CampaignStartTime { get; set; }

        [Column("end_time", TypeName = "INTEGER")]
        public DateTime CampaignEndTime { get; set; }

    }
}
