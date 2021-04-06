using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using PriconneBotConsoleApp.DataTypes;

namespace PriconneBotConsoleApp.Script
{
    public class RediveDatabaseLoader:IDisposable
    {
        private const string SelectScheduleQuery = 
            "SELECT clan_battle_id,release_month, start_time, end_time from clan_battle_schedule" ;

        private SQLiteConnection m_SQLiteConnection;
        public RediveDatabaseLoader()
        {
            m_SQLiteConnection = new SQLiteConnection(); 
            m_SQLiteConnection.ConnectionString = "Data Source = ./data/redive_jp.db; Version = 3";
            m_SQLiteConnection.Open();
        }

        /// <summary>
        /// redive.DBからクラバトの日程を取り出す。
        /// </summary>
        public List<ClanBattleDate> LoadClanBattleSchedule()
        {
            SQLiteCommand command = m_SQLiteConnection.CreateCommand();
            command.CommandText = SelectScheduleQuery;
            var reader = command.ExecuteReader();
            List<ClanBattleDate> schedule = new List<ClanBattleDate>(); 
            while (reader.Read())
            {
                Console.WriteLine(string.Format("月 = {0}, 開始 = {2}, 終了 = {3}",
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetString(3)
                ));
                schedule.Add(new ClanBattleDate() 
                { 
                    ClanBattleID = reader.GetInt32(0), 
                    Month = reader.GetInt32(1),
                    StartBattle = DateTime.Parse(reader.GetString(2)),
                    EndBattle = DateTime.Parse(reader.GetString(3))
                });
            }
            return schedule;
        }

        public void Dispose()
        {
            m_SQLiteConnection.Close();
        }
    }
}
