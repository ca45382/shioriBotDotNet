using System;
using System.Collections.Generic;
using System.Linq;
using ShioriBot.Net.DataType;
using ShioriBot.Net.Define;
using ShioriBot.Net.Extension;

namespace ShioriBot.Net.Model
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
            public int DisplayHP { get { return HP / CommonDefine.DisplayDamageUnit; } }
        }

        public static BossData GetBossData(BossNumberType bossNumberType, int lap)
            => BossDataList.FirstOrDefault(x => x.BossNumber == (byte)bossNumberType && x.LapNumberFrom <= lap && (x.LapNumberTo == -1 || x.LapNumberTo >= lap));

        public static bool ReloadData()
        {
            // TODO : 日本時間に対応
            var nowTime = DateTime.Now;
            using var rediveConnector = new RediveDBContext();

            var clanBattleTrainingSchedule = rediveConnector.ClanBattleTrainingSchedule.AsEnumerable()
                .Where(x => x.BattleStartTime <= nowTime && nowTime <= x.IntervalEndTime)
                .OrderByDescending(x => x.ClanBattleID)
                .FirstOrDefault();

            if (clanBattleTrainingSchedule == null)
            {
                return false;
            }

            var clanBattleSchedule = rediveConnector.ClanBattleSchedule.AsQueryable()
                .First(x => x.ClanBattleID == clanBattleTrainingSchedule.ClanBattleID);

            var clanBattlePeriod = rediveConnector.ClanBattlePeriod.AsQueryable()
                .First(x => x.ClanBattleID == clanBattleTrainingSchedule.ClanBattleID);

            ClanBattleID = clanBattleSchedule.ClanBattleID;
            ReleaseMonth = clanBattleSchedule.ReleaseMonth;
            ClanBattleStartTime = clanBattleSchedule.StartTime;
            ClanBattleEndTime = clanBattlePeriod.BattleEndTime;

            var clanBattleDataArray = rediveConnector.ClanBattleData.AsQueryable()
                .Where(x => x.ClanBattleID == ClanBattleID).OrderBy(x => x.ID)
                .ToArray();
            var bossStatusList = new List<BossData>();

            foreach (var clanBattleData in clanBattleDataArray)
            {
                for (int i = CommonDefine.MinBossNumber; i <= CommonDefine.MaxBossNumber; i++)
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
            IEnumerable<EnemyParameter> enemyDataArray;

            try
            {
                var waveIDArray = bossStatusList.Select(x => x.WaveGroupID).AsEnumerable();
                rediveConnector.WaveGroupData.AsQueryable()
                    .Where(x => waveIDArray.Any(y => y == x.WaveGroupID)).AsEnumerable()
                    .ForEach(x => bossStatusList.Where(y => y.WaveGroupID == x.WaveGroupID).ForEach(y => y.EnemyID = x.EnemyID1));
                var enemyIDList = bossStatusList.Select(x => x.EnemyID);
                enemyDataArray = rediveConnector.EnemyParameter.AsQueryable()
                    .Where(x => enemyIDList.Any(y => y == x.EnemyID))
                    .AsEnumerable();
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
