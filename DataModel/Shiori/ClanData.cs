using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
            _ = bossNum switch
            {
                1 => Boss1Lap = (ushort)bossLap,
                2 => Boss2Lap = (ushort)bossLap,
                3 => Boss3Lap = (ushort)bossLap,
                4 => Boss4Lap = (ushort)bossLap,
                5 => Boss5Lap = (ushort)bossLap,
                _ => 0,
            };
        }

        public int GetBossLap(BossNumberType bossNumberType)
            => GetBossLap((int)bossNumberType);

        public void SetBossLap(BossNumberType bossNumberType, int bossLap)
            => SetBossLap((int)bossNumberType, bossLap);

        /// <summary>
        /// クラン内のチャンネルIDを返す。
        /// </summary>
        /// <param name="channelFeatureType"></param>
        /// <returns></returns>
        public ulong GetChannelID(ChannelFeatureType channelFeatureType)
            => ChannelData?.GetChannelID(ClanID, channelFeatureType) ?? 0;

        /// <summary>
        /// クラン内のメッセージIDを返す。
        /// </summary>
        /// <param name="messageFeatureType"></param>
        /// <returns></returns>
        public ulong GetMessageID(MessageFeatureType messageFeatureType)
            => MessageData?.GetMessageID(ClanID, messageFeatureType) ?? 0;

        /// <summary>
        /// 最も小さい周回数を返す
        /// </summary>
        /// <returns></returns>
        public int GetMinBossLap()
            => Enumerable.Min(new int[] { Boss1Lap, Boss2Lap, Boss3Lap, Boss4Lap, Boss5Lap });

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
                return (byte)BossNumberType.Boss5Number;
            }
            if (Boss1Lap == Boss2Lap + 1)
            {
                return (byte)BossNumberType.Boss1Number;
            }
            else if (Boss2Lap == Boss3Lap + 1)
            {
                return (byte)BossNumberType.Boss2Number;
            }
            else if (Boss3Lap == Boss4Lap + 1)
            {
                return (byte)BossNumberType.Boss3Number;
            }
            else if (Boss4Lap == Boss5Lap + 1)
            {
                return (byte)BossNumberType.Boss4Number;
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
                return Boss5Lap;
            }
            else
            {
                return Boss1Lap;
            }
        }
    }
}
