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
        public List<RoleData> RoleData { get; set; }
        public List<PlayerData> PlayerData { get; set; }

        public int GetBossLap(int bossNumber)
        {
            return bossNumber switch
            {
                1 => Boss1Lap,
                2 => Boss2Lap,
                3 => Boss3Lap,
                4 => Boss4Lap,
                5 => Boss5Lap,
                _ => 0,
            };
        }

        public void SetBossLap(int bossNum, int bossLap)
        {
            switch (bossNum)
            {
                case 1:
                    Boss1Lap = (ushort)bossLap;
                    return;
                case 2:
                    Boss2Lap = (ushort)bossLap;
                    return;
                case 3:
                    Boss3Lap = (ushort)bossLap;
                    return;
                case 4:
                    Boss4Lap = (ushort)bossLap;
                    return;
                case 5:
                    Boss5Lap = (ushort)bossLap;
                    return;
                default:
                    return;
            }
        }

        /// <summary>
        /// 5つのボスデータから今のボスに変換。来月削除。
        /// </summary>
        /// <param name="clanData"></param>
        /// <returns></returns>
        [Obsolete]
        public byte GetNowBoss()
        {
            if (Boss1Lap == Boss2Lap
                && Boss2Lap == Boss3Lap
                && Boss3Lap == Boss4Lap
                && Boss4Lap == Boss5Lap)
            {
                return 1;
            }
            if (Boss1Lap - 1 == Boss2Lap)
            {
                return 2;
            }
            else if (Boss2Lap - 1 == Boss3Lap)
            {
                return 3;
            }
            else if (Boss3Lap - 1 == Boss4Lap)
            {
                return 4;
            }
            else if (Boss4Lap - 1 == Boss5Lap)
            {
                return 5;
            }

            return 0;
        }

        /// <summary>
        /// 5つのボスデータから今のLapに変換。来月削除。
        /// </summary>
        /// <param name="clanData"></param>
        /// <returns></returns>
        [Obsolete]
        public ushort GetNowLap()
        {
            if (Boss1Lap == Boss2Lap
                && Boss2Lap == Boss3Lap
                && Boss3Lap == Boss4Lap
                && Boss4Lap == Boss5Lap)
            {
                return Boss1Lap;
            }
            else
            {
                return Boss5Lap;
            }
        }
    }

}
