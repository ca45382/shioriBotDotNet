using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataModel
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

        [Column("boss_1_lap", TypeName = "SMALLINT UNSIGNED")]
        public ushort Boss1Lap { get; set; }

        [Column("boss_2_lap", TypeName = "SMALLINT UNSIGNED")]
        public ushort Boss2Lap { get; set; }

        [Column("boss_3_lap", TypeName = "SMALLINT UNSIGNED")]
        public ushort Boss3Lap { get; set; }

        [Column("boss_4_lap", TypeName = "SMALLINT UNSIGNED")]
        public ushort Boss4Lap { get; set; }

        [Column("boss_5_lap", TypeName = "SMALLINT UNSIGNED")]
        public ushort Boss5Lap { get; set; }

        [Column("reservation_lap", TypeName = "TINYINT UNSIGNED")]
        public byte ReservationLap { get; set; }

        [Column("reservation_start_time", TypeName = "TIME")]
        public TimeSpan ReservationStartTime { get; set; }

        [Column("reservation_end_time", TypeName = "TIME")]
        public TimeSpan ReservationEndTime { get; set; }

        /// <summary>
        /// 外部キー
        /// </summary>
        [Column("server_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong ServerID { get; set; }

        public ServerData ServerData { get; set; }
        public List<ChannelData> ChannelData { get; set; }
        public List<MessageData> MessageData { get; set; }
        public List<RoleData> roleData { get; set; }
        public List<PlayerData> PlayerData { get; set; }
    }

}
