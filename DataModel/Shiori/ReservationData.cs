using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataTypes
{
    [Table("reserve_data")]
    public class ReservationData
    {
        [Column("reserve_id", TypeName = "BIGINT UNSIGNED"), Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong ReserveID { get; set; }

        [Column("create_date_time"), Required]
        [Timestamp]
        public DateTime CreateDateTime { get; set; }

        [Column("update_date_time"), Required]
        [Timestamp]
        public DateTime UpdateDateTime { get; set; }

        [Column("boss_num", TypeName = "TINYINT UNSIGNED")]
        public byte BossNumber { get; set; }

        [Column("battle_lap", TypeName = "TINYINT UNSIGNED")]
        public byte BattleLap { get; set; }

        [Column("attack_type", TypeName = "TINYINT UNSIGNED")]
        public byte AttackType { get; set; }

        [Column("carry_over_flag", TypeName = "TINYINT UNSIGNED")]
        public bool CarryOverFlag { get; set; }

        [Column("reply", TypeName = "TINYINT UNSIGNED")]
        public bool Reply { get; set; }

        [Column("comment_data", TypeName = "varchar(100)")]
        public string CommentData { get; set; }

        [Column("delete_flag", TypeName = "TINYINT UNSIGNED")]
        public bool DeleteFlag { get; set; }

        [Column("player_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong PlayerID { get; set; }

        public PlayerData PlayerData { get; set; }
    }
}
