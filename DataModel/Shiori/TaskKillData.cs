using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataModel
{
    [Table("task_kill_data")]
    public class TaskKillData
    {
        [Column("task_kill_id", TypeName = "BIGINT UNSIGNED"), Required, Key]
        public ulong TaskKillID { get; set; }

        [Column("player_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong PlayerID { get; set; }

        [Column("date_time", TypeName = "TIMESTAMP"), Required]
        public DateTime DateTime { get; set; }

        [Column("delete_flag", TypeName = "TINYINT UNSIGNED"), Required]
        public bool DeleteFlag { get; set; }

        public PlayerData PlayerData { get; set; }
    }
}
