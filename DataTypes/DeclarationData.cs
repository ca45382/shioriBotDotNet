using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataTypes
{
    [Table("declare_data")]
    public class DeclarationData
    {

        [Column("declare_id", TypeName = "bigint(20) unsigned"), Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong DeclareID { get; set; }

        [Column("date_time"), Required]
        [Timestamp]
        public byte[] DateTime { get; set; }

        [Column("boss_num", TypeName = "int(1)")]
        public int BossNumber { get; set; }

        [Column("battle_lap", TypeName = "int(3)")]
        public int BattleLap { get; set; }

        [Column("attack_type", TypeName = "int(2)")]
        public int AttackType { get; set; }

        [Column("finish_flag", TypeName = "tinyint(4)")]
        public bool FinishFlag { get; set; }

        [Column("delete_flag", TypeName = "tinyint(4)")]
        public bool DeleteFlag { get; set; }


        [Column("player_id", TypeName = "bigint(20) unsigned"), Required]
        public ulong PlayerID { get; set; }

        public PlayerData PlayerData { get; set; }
    }
}
