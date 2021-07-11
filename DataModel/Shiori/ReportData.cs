using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataModel
{
    [Table("report_data")]
    public class ReportData
    {
        [Column("report_id", TypeName = "BIGINT UNSIGNED"), Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong ReportID { get; set; }

        [Column("player_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong PlayerID { get; set; }

        [Column("date_time"), Required]
        [Timestamp]
        public DateTime DateTime { get; set; }

        [Column("final_damage", TypeName = "INT UNSIGNED")]
        public uint FinalDamage { get; set; }

        [Column("boss_num", TypeName = "TINYINT UNSIGNED")]
        public byte BossNumber { get; set; }

        [Column("battle_lap", TypeName = "SMALLINT UNSIGNED")]
        public ushort BattleLap { get; set; }

        [Column("attack_type", TypeName = "TINYINT UNSIGNED")]
        public byte AttackType { get; set; }

        /// <summary>
        /// この本戦で討伐したかどうか判定
        /// </summary>
        [Column("subdue_flag", TypeName = "TINYINT UNSIGNED")]
        public bool SubdueFlag { get; set; }

        /// <summary>
        /// 有効データかどうか判定。
        /// 集計機能の際に活きてくる。
        /// </summary>
        [Column("valid_flag", TypeName = "TINYINT UNSIGNED")]
        public bool ValidFlag { get; set; }

        [Column("delete_flag", TypeName = "TINYINT UNSIGNED")]
        public bool DeleteFlag { get; set; }

        public PlayerData PlayerData { get; set; }
    }
}
