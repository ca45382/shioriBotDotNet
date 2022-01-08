using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShioriBot.Net.Model
{
    [Table("declare_data")]
    public class DeclarationData
    {
        [Column("declare_id", TypeName = "BIGINT UNSIGNED"), Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong DeclareID { get; set; }

        [Column("date_time"), Required]
        [Timestamp]
        public DateTime DateTime { get; set; }

        [Column("boss_num", TypeName = "TINYINT UNSIGNED")]
        public byte BossNumber { get; set; }

        [Column("battle_lap", TypeName = "SMALLINT UNSIGNED")]
        public ushort BattleLap { get; set; }

        [Column("attack_type", TypeName = "TINYINT UNSIGNED")]
        public byte AttackType { get; set; }

        [Column("finish_flag", TypeName = "TINYINT UNSIGNED")]
        public bool FinishFlag { get; set; }

        [Column("delete_flag", TypeName = "TINYINT UNSIGNED")]
        public bool DeleteFlag { get; set; }

        [Column("player_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong PlayerID { get; set; }

        public PlayerData PlayerData { get; set; }
    }
}
