using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.Model
{
    [Table("clan_battle_training_schedule")]
    public class ClanBattleTrainingSchedule
    {
        [Column("clan_battle_id", TypeName = "INTEGER"), Key]
        public int ClanBattleID { get; set; }

        [Column("battle_start_time", TypeName = "TEXT"), Timestamp]
        public DateTime BattleStartTime { get; set; }

        [Column("battle_end_time", TypeName = "TEXT"), Timestamp]
        public DateTime BattleEndTime { get; set; }

        [Column("interval_end_time", TypeName = "TEXT"), Timestamp]
        public DateTime IntervalEndTime { get; set; }
    }
}
