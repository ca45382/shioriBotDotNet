using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShioriBot.Net.Model
{
    [Table("clan_battle_schedule")]
    public class ClanBattleSchedule
    {
        [Column("clan_battle_id", TypeName = "INTEGER"), Key]
        public int ClanBattleID { get; set; }

        [Column("release_month", TypeName = "INTEGER")]
        public int ReleaseMonth { get; set; }

        [Column("last_clan_battle_id", TypeName = "INTEGER")]
        public int LastClanBattleID { get; set; }

        [Column("start_time", TypeName = "TEXT")]
        [Timestamp]
        public DateTime StartTime { get; set; }

        [Column("end_time", TypeName = "TEXT")]
        [Timestamp]
        public DateTime EndTime { get; set; }
    }
}
