using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using ShioriBot.Net.Database;
using ShioriBot.Net.Model;
using ShioriBot.Net.DataType;
using ShioriBot.Net.Define;

namespace ShioriBot.Net.Script
{
    public class UpdateDate
    {
        private readonly DateTime m_BoundaryTime;
        private readonly IEnumerable<ClanData> m_ClanList;
        private readonly DiscordSocketClient m_Client;

        public UpdateDate(DiscordSocketClient client)
        {
            m_BoundaryTime = DateTime.Today + TimeDefine.GameDateOffset;
            m_ClanList = DatabaseClanDataController.LoadClanData();
            m_Client = client;
        }

        public async Task DeleteYesterdayData()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var nowTime = DateTime.Now;

            var reservarionDataList = DatabaseReservationController.LoadReservationData()
                .Where(x => x.CreateDateTime < m_BoundaryTime);
            var carryOverList = DatabaseCarryOverController.GetCarryOverData()
                .Where(x => x.DateTime < m_BoundaryTime);
            var taskKillList = DatabaseTaskKillController.LoadTaskKillData()
                .Where(x => x.DateTime < m_BoundaryTime);
            var reportList = DatabaseReportDataController.GetReportData()
                .Where(x => x.DateTime < m_BoundaryTime);

            DatabaseReservationController.DeleteReservationData(reservarionDataList);
            DatabaseCarryOverController.DeleteCarryOverData(carryOverList);
            DatabaseTaskKillController.DeleteTaskKillData(taskKillList);
            DatabaseReportDataController.DeleteReportData(reportList, true);

            foreach (var clanData in m_ClanList)
            {
                List<Task> taskList = new();
                var guild = m_Client.GetGuild(clanData.ServerID);
                var clanRole = guild?.GetRole(clanData.ClanRoleID);

                if (clanRole == null)
                {
                    continue;
                }

                taskList.Add(new BattleTaskKill(clanRole).SyncTaskKillData());

                if (RediveClanBattleData.ClanBattleStartTime <= nowTime && nowTime <= RediveClanBattleData.ClanBattleEndTime)
                {
                    taskList.Add(new BattleReservationSummary(clanRole).UpdateMessage());

                    for (int i = 0; i < CommonDefine.MaxBossNumber; i++)
                    {
                        taskList.Add(new BattleDeclaration(clanRole, (BossNumberType)(i + 1)).UpdateDeclarationBotMessage());
                        // TODO:ここに進行の方も追加
                    }
                }

                await Task.WhenAll(taskList);
            }

            sw.Stop();

            // 結果表示
            Console.WriteLine("■処理Aにかかった時間");
            TimeSpan ts = sw.Elapsed;
            Console.WriteLine($"　{ts}");
            Console.WriteLine($"　{ts.Hours}時間 {ts.Minutes}分 {ts.Seconds}秒 {ts.Milliseconds}ミリ秒");
            Console.WriteLine($"　{sw.ElapsedMilliseconds}ミリ秒");
        }
    }
}
