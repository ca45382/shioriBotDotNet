using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace PriconneBotConsoleApp.Script
{
    public class LoadRediveSQLiteData
    {

        private SQLiteConnection m_sQLiteConnection;
        public LoadRediveSQLiteData()
        {
            m_sQLiteConnection = new SQLiteConnection(); 
            m_sQLiteConnection.ConnectionString = "Data Source = ./data/redive_jp.db; Version = 3";
            return;
        }

        /// <summary>
        /// redive.DBへの接続
        /// </summary>
        public void open()
        {
            m_sQLiteConnection.Open();
        }

        /// <summary>
        /// redive.DBへの接続を解除
        /// </summary>
        public void close()
        {
            m_sQLiteConnection.Close();
        }

        /// <summary>
        /// redive.DBからクラバトの日程を取り出す。
        /// </summary>
        public List<DataSet.ClanBattleDate> loadClanBattleScadule()
        {
            SQLiteCommand command = m_sQLiteConnection.CreateCommand();
            command.CommandText =
                "SELECT clan_battle_id,release_month, start_time, end_time from clan_battle_schedule";
            var render = command.ExecuteReader();
            List<DataSet.ClanBattleDate> schedule = new List<DataSet.ClanBattleDate>(); 
            while (render.Read())
            {
                Console.WriteLine(string.Format("月 = {0}, 開始 = {1}, 終了 = {2}",
                        render.GetInt32(0),
                        render.GetInt32(1),
                        render.GetString(2),
                        render.GetString(3)
                    ));
                schedule.Add(new DataSet.ClanBattleDate() 
                { 
                    ClanBattleID = render.GetInt32(0), 
                    Month = render.GetInt32(1),
                    StartBattle = render.GetString(2),
                    EndBattle = render.GetString(3)
                });
            }
            return schedule;
        }

    }
}
