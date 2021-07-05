using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataModel
{
    [Table("progress_data")]
    public class ProgressData
    {
        [Column("progress_id", TypeName = "BIGINT UNSIGNED"), Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong ProgressID { get; set; }

        /// <summary>
        /// 外部キー
        /// </summary>
        [Column("player_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong PlayerID { get; set; }

        [Column("date_time"), Required]
        [Timestamp]
        public DateTime CreateDateTime { get; set; }

        [Column("damage", TypeName = "INT UNSIGNED")]
        public uint Damage { get; set; }

        [Column("boss_num", TypeName = "TINYINT UNSIGNED")]
        public byte BossNumber { get; set; }

        [Column("battle_lap", TypeName = "SMALLINT UNSIGNED")]
        public ushort BattleLap { get; set; }

        [Column("attack_type", TypeName = "TINYINT UNSIGNED")]
        public byte AttackType { get; set; }

        /// <summary>
        /// 0:なし 1:凸確定 2:凸仮確定 3:SOS 4:持ち越し
        /// </summary>
        [Column("status", TypeName = "TINYINT UNSIGNED")]
        public byte Status { get; set; }

        [Column("comment_data", TypeName = "varchar(100)")]
        public string CommentData { get; set; }

        [Column("delete_flag", TypeName = "TINYINT UNSIGNED")]
        public bool DeleteFlag { get; set; }

        public PlayerData PlayerData { get; set; }
    }
}
