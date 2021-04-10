using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataTypes
{
    [Table("reserve_data")]
    public class ReservationData
    {
        [Column("reserve_id", TypeName = "BIGINT UNSIGNED AUTO_INCREMENT")]
        public int ReserveID { get; set; }

        [Column("clan_role_id", TypeName = "BIGINT UNSIGNED"), Required]
        public int PlayerID { get; set; }

        [Column("date_time", TypeName = "TIMESTAMP ON UPDATE CURRENT_TIMESTAMP"), Required]
        public DateTime DateTime { get; set; }

        [Column("boss_num", TypeName = "int(1)")]
        public int BossNumber { get; set; }

        [Column("battle_lap", TypeName = "int(3)")]
        public int BattleLaps { get; set; }

        [Column("attack_type", TypeName = "int(2)")]
        public int AttackType { get; set; }

        [Column("reply", TypeName = "TINYINT")]
        public bool Reply { get; set; }

        [Column("comment_data", TypeName = "VARCHAR(100)")]
        public string CommentData { get; set; }

        [Column("delete_flag", TypeName = "TINYINT")]
        public bool DeleteFlag { get; set; }


        public ClanData ClanData { get; set; }

    }
}
