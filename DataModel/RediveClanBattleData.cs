using System;
using System.Collections.Generic;
using System.Linq;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Define;

namespace PriconneBotConsoleApp.DataModel
{
    public static class RediveClanBattleData
    {
        public static int ClanBattleID { get; private set; } = 0;
        public static int ReleaseMonth { get; private set; } = 0;
        public static DateTime ClanBattleStartTime { get; private set; }
        public static DateTime ClanBattleEndTime { get; private set; }
        public static IEnumerable<BossData> BossDataList { get; private set; }

        public class BossData
        {
            public int LapNumberFrom;
            public int LapNumberTo;
            public int Phase;
            public byte BossNumber;
            public int WaveGroupID;
            public int EnemyID;
            public string Name;
            public int Level;
            public int HP;
            public float ScoreCoefficient;
        }

        public static bool ReloadData()
        {
            var nowTime = DateTime.Now;
            using var rediveConnector = new RediveConnector();

            var clanBattleSchedule = rediveConnector.ClanBattleSchedule.AsEnumerable()
                .Where(x => nowTime >= x.StartTime && nowTime <= x.EndTime)
                .FirstOrDefault();

            if (clanBattleSchedule == null)
            {
                return false;
            }

            ClanBattleID = clanBattleSchedule.ClanBattleID;
            ReleaseMonth = clanBattleSchedule.ReleaseMonth;
            ClanBattleStartTime = clanBattleSchedule.StartTime;
            ClanBattleEndTime = clanBattleSchedule.EndTime;

            var clanBattleDataList = rediveConnector.ClanBattleData.AsQueryable()
                .Where(x => x.ClanBattleID == ClanBattleID)
                .ToList();
            var bossStatusList = new List<BossData>();

            foreach (var clanBattleData in clanBattleDataList)
            {
                for (int i = Common.MinBossNumber; i <= Common.MaxBossNumber; i++)
                {
                    bossStatusList.Add(new BossData()
                    {
                        LapNumberFrom = clanBattleData.LapNumberFrom,
                        LapNumberTo = clanBattleData.LapNumberTo,
                        BossNumber = (byte)i,
                        WaveGroupID = clanBattleData.GetWaveGroupID((BossNumberType)i),
                        ScoreCoefficient = clanBattleData.GetScoreCoefficient((BossNumberType)i),
                        Phase = clanBattleData.Phase,
                    });
                }
            }


            // クラバトデータからボス情報を抽出
            EnemyParameter[] enemyDataArray;
            try
            {
                var waveIDArray = bossStatusList.Select(x => x.WaveGroupID).ToArray();
                rediveConnector.WaveGroupData.AsQueryable()
                    .Where(x => waveIDArray.Any(y => y == x.WaveGroupID)).ToList()
                    .ForEach(x => bossStatusList.Where(y => y.WaveGroupID == x.WaveGroupID).ToList().ForEach(y => y.EnemyID = x.EnemyID1));
                var enemyIDList = bossStatusList.Select(x => x.EnemyID);
                enemyDataArray = rediveConnector.EnemyParameter.AsQueryable()
                    .Where(x => enemyIDList.Any(y => y == x.EnemyID))
                    .ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            foreach (var bossStatus in bossStatusList)
            {
                var enemyData = enemyDataArray.FirstOrDefault(x => x.EnemyID == bossStatus.EnemyID);

                if (enemyData == null)
                {
                    continue;
                }

                bossStatus.Name = enemyData.Name;
                bossStatus.HP = enemyData.HP;
                bossStatus.Level = enemyData.Level;
            }

            BossDataList = bossStatusList;
            return true;
        }
    }
}
