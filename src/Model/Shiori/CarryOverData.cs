using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.Model
{
    [Table("carry_over_data")]
    public class CarryOverData
    {
        [Column("carry_over_id", TypeName = "BIGINT UNSIGNED"), Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong CarryOverID { get; set; }

        [Column("player_id", TypeName = "BIGINT UNSIGNED")]
        public ulong PlayerID { get; set; }

        [Column("date_time"), Required]
        [Timestamp]
        public DateTime DateTime { get; set; }

        [Column("remain_time", TypeName = "TINYINT UNSIGNED")]
        public byte RemainTime { get; set; }

        [Column("boss_num", TypeName = "TINYINT UNSIGNED")]
        public byte BossNumber { get; set; }

        [Column("battle_lap", TypeName = "SMALLINT UNSIGNED")]
        public ushort BattleLap { get; set; }

        [Column("report_id", TypeName = "BIGINT UNSIGNED")]
        public ulong ReportID { get; set; }

        [Column("comment_data", TypeName = "varchar(100)")]
        public string CommentData { get; set; }

        [Column("delete_flag", TypeName = "TINYINT UNSIGNED")]
        public bool DeleteFlag { get; set; }

        public PlayerData PlayerData { get; set; }
    }
}
