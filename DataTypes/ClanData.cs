using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataTypes
{
    [Table("clan_info")]
    public class ClanData
    {
        [Column("clan_id", TypeName = "BIGINT UNSIGNED AUTO_INCREMENT"), Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong ClanID { get; set; }

        [Column("clan_role_id", TypeName = "BIGINT UNSIGNED")]
        public ulong ClanRoleID { get; set; }

        [Column("clan_name", TypeName = "VARCHAR(30)")]
        public string ClanName { get; set; }

        [Column("battle_lap", TypeName = "TINYINT UNSIGNED")]
        public byte BattleLap { get; set; }

        [Column("boss_num", TypeName = "TINYINT UNSIGNED")]
        public byte BossNumber { get; set; }

        [Column("progress_flag", TypeName = "TINYINT UNSIGNED")]
        public bool ProgressiveFlag { get; set; }

        [Column("reservation_lap", TypeName = "TINYINT UNSIGNED")]
        public byte ReservationLap { get; set; }

        [Column("reservation_start_time ", TypeName = "TIME")]
        public byte[] ReservationStartTime { get; set; }

        [Column("reservation_end_time ", TypeName = "TIME")]
        public byte[] ReservationEndTime { get; set; }

        /// <summary>
        /// 外部キー
        /// </summary>
        [Column("server_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong ServerID { get; set; }

        public BotDatabase BotDatabase { get; set; }

        // リレーション
        public ChannelIDs ChannelIDs { get; set; }

        public MessageIDs MessageIDs { get; set; }

        public RoleIDs RoleIDs { get; set; }

        public List<PlayerData> PlayerData { get; set; }
    }

    [Table("clan_channel")]
    public class ChannelIDs
    {
        [Column("progress_id", TypeName = "BIGINT UNSIGNED")]
        public ulong ProgressiveChannelID { get; set; }

        [Column("report_id", TypeName = "BIGINT UNSIGNED")]
        public ulong ReportChannelID { get; set; }
        
        [Column("carry_over_id", TypeName = "BIGINT UNSIGNED")]
        public ulong CarryOverChannelID { get; set; }
        
        [Column("task_kill_id", TypeName = "BIGINT UNSIGNED")]
        public ulong TaskKillChannelID { get; set; }
        
        [Column("declare_id", TypeName = "BIGINT UNSIGNED")]
        public ulong DeclarationChannelID { get; set; }
        
        [Column("reserve_id", TypeName = "BIGINT UNSIGNED")]
        public ulong ReservationChannelID { get; set; }

        [Column("reserve_result_id", TypeName = "BIGINT UNSIGNED")]
        public ulong ReservationResultChannelID { get; set; }
        
        [Column("tl_time_id", TypeName = "BIGINT UNSIGNED")]
        public ulong TimeLineConversionChannelID { get; set; }

        [Column("clan_id", TypeName = "BIGINT UNSIGNED"), Key]
        public ulong ClanID { get; set; }

        public ClanData ClanData { get; set; }
    }

    [Table("clan_message")]
    public class MessageIDs
    {
        [Column("progress_msg_id", TypeName = "BIGINT UNSIGNED")]
        public ulong ProgressiveMessageID { get; set; }
        
        [Column("declare_msg_id", TypeName = "BIGINT UNSIGNED")]
        public ulong DeclarationMessageID { get; set; }

        [Column("reserve_msg_id", TypeName = "BIGINT UNSIGNED")]
        public ulong ReservationMessageID { get; set; }

        [Column("clan_id", TypeName = "BIGINT UNSIGNED"), Key]
        public ulong ClanID { get; set; }

        public ClanData ClanData { get; set; }
    }

    [Table("clan_role")]
    public class RoleIDs
    {
        [Column("task_kill_role_id", TypeName = "BIGINT UNSIGNED")]
        public ulong TaskKillRoleID { get; set; }

        [Column("boss_1_role_id", TypeName = "BIGINT UNSIGNED")]
        public ulong FirstBossID { get; set; }

        [Column("boss_2_role_id", TypeName = "BIGINT UNSIGNED")]
        public ulong SecondBossID { get; set; }
        
        [Column("boss_3_role_id", TypeName = "BIGINT UNSIGNED")]
        public ulong ThirdBossID { get; set; }
        
        [Column("boss_4_role_id", TypeName = "BIGINT UNSIGNED")]
        public ulong FourthBossID { get; set; }
        
        [Column("boss_5_role_id", TypeName = "BIGINT UNSIGNED")]
        public ulong FifthBossID { get; set; }

        [Column("clan_id", TypeName = "BIGINT UNSIGNED"), Key]
        public ulong ClanID { get; set; }

        public ClanData ClanData { get; set; }
    }
}
