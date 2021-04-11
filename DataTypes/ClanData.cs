using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

using Microsoft.EntityFrameworkCore;

namespace PriconneBotConsoleApp.DataTypes
{
    [Table("clan_info")]
    public class ClanData
    {

        [Column("clan_id", TypeName = "bigint(20) unsigned"), Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong ClanID { get; set; }

        [Column("clan_role_id", TypeName = "varchar(21)")]
        public string ClanRoleID { get; set; }

        [Column("clan_name", TypeName = "varchar(100)")]
        public string ClanName { get; set; }

        [Column("battle_lap", TypeName = "int(3)")]
        public int BattleLap { get; set; }

        [Column("boss_num", TypeName = "int(1)")]
        public int BossNumber { get; set; }

        [Column("progress_flag", TypeName = "tinyint(4)")]
        public bool ProgressiveFlag { get; set; }

        [Column("boss_role_ready", TypeName = "tinyint(4)")]
        public bool BossRoleReady { get; set; }

        // 外部キー
        [Column("server_id", TypeName = "varchar(21)"), Required]
        public string ServerID { get; set; }


        public BotDatabase BotDatabase { get; set; }

        //リレーション
        public ChannelIDs ChannelIDs { get; set; }

        public MessageIDs MessageIDs { get; set; }

        public RoleIDs RoleIDs { get; set; }

        public List<PlayerData> PlayerData { get; set; }

        
    }

    [Table("clan_channel")]
    public class ChannelIDs
    {
        [Column("progress_id", TypeName = "varchar(21)")]
        public string ProgressiveChannelID;

        [Column("report_id", TypeName = "varchar(21)")]
        public string ReportChannelID;
        
        [Column("carry_over_id", TypeName = "varchar(21)")]
        public string CarryOverChannelID;
        
        [Column("task_kill_id", TypeName = "varchar(21)")]
        public string TaskKillChannelID;
        
        [Column("declare_id", TypeName = "varchar(21)")]
        public string DeclarationChannelID;
        
        [Column("reserve_id", TypeName = "varchar(21)")]
        public string ReservationChannelID;
        
        [Column("tl_time_id", TypeName = "varchar(21)")]
        public string TimeLineConversionChannelID;

        [Column("clan_id", TypeName = "bigint(20) unsigned"), Key]
        public ulong ClanID { get; set; }

        public ClanData ClanData { get; set; }
    }

    [Table("clan_message")]
    public class MessageIDs
    {
        [Column("progress_msg_id", TypeName = "varchar(21)")]
        public string ProgressiveMessageID;
        
        [Column("declare_msg_id", TypeName = "varchar(21)")]
        public string DeclarationMessageID;

        [Column("reserve_msg_id", TypeName = "varchar(21)")]
        public string ReservationMessageID;

        [Column("clan_id", TypeName = "bigint(20) unsigned"), Key]
        public ulong ClanID { get; set; }


        public ClanData ClanData { get; set; }
    }

    [Table("clan_role")]
    public class RoleIDs
    {
        [Column("task_kill_role_id", TypeName = "varchar(21)")]
        public string TaskKillRoleID;

        [Column("boss_1_role_id", TypeName = "varchar(21)")]
        public string FirstBossID;

        [Column("boss_2_role_id", TypeName = "varchar(21)")]
        public string SecondBossID;
        
        [Column("boss_3_role_id", TypeName = "varchar(21)")]
        public string ThirdBossID;
        
        [Column("boss_4_role_id", TypeName = "varchar(21)")]
        public string FourthBossID;
        
        [Column("boss_5_role_id", TypeName = "varchar(21)")]
        public string FifthBossID;

        [Column("clan_id", TypeName = "bigint(20) unsigned"), Key]
        public ulong ClanID { get; set; }

        public ClanData ClanData { get; set; }
    }
}
