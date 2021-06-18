using PriconneBotConsoleApp.DataModel;
using System;
using System.Collections.Generic;

namespace PriconneBotConsoleApp.Extension
{
    public static class ClanDataExtension
    {
        public static List<ushort> GetBossLap(this ClanData clanData)
        {
            return new List<ushort>
            {
                clanData.Boss1Lap,
                clanData.Boss2Lap,
                clanData.Boss3Lap,
                clanData.Boss4Lap,
                clanData.Boss5Lap
            };
        }

        public static int GetBossLap(this ClanData clanData, int bossNumber)
        {
            return bossNumber switch
            {
                1 => clanData.Boss1Lap,
                2 => clanData.Boss2Lap,
                3 => clanData.Boss3Lap,
                4 => clanData.Boss4Lap,
                5 => clanData.Boss5Lap,
                _ => 0,
            };
        }

        public static void SetBossLap(this ClanData clanData, int bossNum, int bossLap)
        {
            switch (bossNum)
            {
                case 1:
                    clanData.Boss1Lap = (ushort)bossLap;
                    return;
                case 2:
                    clanData.Boss2Lap = (ushort)bossLap;
                    return;
                case 3:
                    clanData.Boss3Lap = (ushort)bossLap;
                    return;
                case 4:
                    clanData.Boss4Lap = (ushort)bossLap;
                    return;
                case 5:
                    clanData.Boss5Lap = (ushort)bossLap;
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
        public static byte GetNowBoss(this ClanData clanData)
        {
            if(clanData.Boss1Lap == clanData.Boss2Lap 
                && clanData.Boss2Lap == clanData.Boss3Lap 
                && clanData.Boss3Lap == clanData.Boss4Lap
                && clanData.Boss4Lap == clanData.Boss5Lap)
            {
                return 1;
            }
            if(clanData.Boss1Lap - 1 == clanData.Boss2Lap)
            {
                return 2;
            }
            else if(clanData.Boss2Lap - 1 == clanData.Boss3Lap)
            {
                return 3;
            }
            else if(clanData.Boss3Lap - 1 == clanData.Boss4Lap)
            {
                return 4;
            }
            else if(clanData.Boss4Lap - 1 == clanData.Boss5Lap)
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
        public static ushort GetNowLap(this ClanData clanData)
        {
            if (clanData.Boss1Lap == clanData.Boss2Lap
                && clanData.Boss2Lap == clanData.Boss3Lap
                && clanData.Boss3Lap == clanData.Boss4Lap
                && clanData.Boss4Lap == clanData.Boss5Lap)
            {
                return clanData.Boss1Lap;
            }else
            {
                return clanData.Boss5Lap;
            }
        }
    }
}
