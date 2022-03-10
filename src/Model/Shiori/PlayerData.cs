using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShioriBot.Model
{
    [Table("player_data")]
    public class PlayerData
    {
        [Column("player_id", TypeName = "BIGINT UNSIGNED"), Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong PlayerID { get; set; }

        [Column("user_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong UserID { get; set; }

        [Column("name", TypeName = "VARCHAR(100)")]
        public string GuildUserName { get; set; }

        /// <summary>
        /// 外部キー
        /// </summary>
        [Column("clan_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong ClanID { get; set; }

        public ClanData ClanData { get; set; }

        public List<ReservationData> ReservationData { get; set; }
        public List<DeclarationData> DeclarationData { get; set; }
        public List<ReportData> ReportData { get; set; }
        public List<ProgressData> ProgressData { get; set; }
        public List<CarryOverData> CarryOverData { get; set; }
        public List<TaskKillData> TaskKillData { get; set; }
    }
}
