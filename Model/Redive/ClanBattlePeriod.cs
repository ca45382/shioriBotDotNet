using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.Model
{
    [Table("clan_battle_period")]
    public class ClanBattlePeriod
    {
        [Column("clan_battle_id", TypeName = "INTEGER"), Key]
        public int ClanBattleID { get; set; }

        [Column("start_time", TypeName = "TEXT")]
        [Timestamp]
        public DateTime StartTime { get; set; }

        [Column("end_time", TypeName = "TEXT")]
        [Timestamp]
        public DateTime BattleEndTime { get; set; }

        [Column("interval_end", TypeName = "TEXT")]
        [Timestamp]
        public DateTime IntervalEndTime { get; set; }
    }
}
