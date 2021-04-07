using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using MySql.Data.MySqlClient;
using Discord.WebSocket;

using PriconneBotConsoleApp.Script;
using PriconneBotConsoleApp.DataTypes;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLConnector:IDisposable
    {
        private protected MySqlConnection m_mySQLConnection;


        /// <summary>
        /// MySQLと接続するためのメソッド
        /// </summary>
        public MySQLConnector()
        {
            var jsonData = new JsonDataManager();
            var connectorString = jsonData.MySQLConnectionString();

            m_mySQLConnection = new MySqlConnection(connectorString);
            m_mySQLConnection.Open();
        }

        /// <summary>
        /// MySQLとの通信を終了する際のメソッド
        /// </summary>
        public void Dispose()
        {
            //throw new NotImplementedException();
            m_mySQLConnection.Close();

        }

        /// <summary>
        /// クランの情報をSQLから受け取って返す
        /// </summary>
        /// <returns>ClanInfoのListで返す</returns>
        public List<ClanData> LoadClanInfo()
        {
            var commandString = 
                "SELECT server_id, clan_role_id from clan_info";

            var command = new MySqlCommand();
            command.Connection = m_mySQLConnection;
            command.CommandText = commandString;

            var sqlDataReader = command.ExecuteReader();
            var ClanData = new List<ClanData>();
            while(sqlDataReader.Read())
            {
                ClanData.Add(new ClanData()
                {
                    ServerID = sqlDataReader.GetString(0),
                    ClanRoleID = sqlDataReader.GetString(1)
                });
            }
            sqlDataReader.Close();

            return ClanData;
        }

        public ClanData LoadClanInfo(SocketRole role)
        {
            var clanData = new ClanData();
            var loadClanInfoCommandString =
               "SELECT server_id, clan_role_id from clan_info " +
               "WHERE server_id = @guildID AND clan_role_id = @clanRoleID";

            var loadClanChannelCommandString =
                "SELECT progress_id, report_id, carry_over_id, " +
                "task_kill_id, declare_id, reserve_id, tl_time_id " +
                "FROM clan_channel " +
                "WHERE server_id = @guildID AND clan_role_id = @clanRoleID";

            var command = new MySqlCommand(
                loadClanInfoCommandString, m_mySQLConnection
            );
            command.Parameters.Add(
                new MySqlParameter("guildID", role.Guild.Id.ToString())
                );
            command.Parameters.Add(
                new MySqlParameter("clanroleID", role.Id.ToString())
                );

            var sqlDataReader = command.ExecuteReader();
            while (sqlDataReader.Read())
            {
                clanData.ServerID = (sqlDataReader.GetValue(0) == null) ? null : sqlDataReader.GetString(0);
                clanData.ClanRoleID = (sqlDataReader.GetValue(1) == null) ? null : sqlDataReader.GetString(1);
            }
            sqlDataReader.Close();

            command.CommandText = loadClanChannelCommandString;

            sqlDataReader = command.ExecuteReader();
            while (sqlDataReader.Read())
            {
                clanData.ChannelIDs.ProgressiveChannelID =
                    (sqlDataReader.GetValue(0) == DBNull.Value) ? null : sqlDataReader.GetString(0);
                clanData.ChannelIDs.ReportChannelID =
                    (sqlDataReader.GetValue(1) == DBNull.Value) ? null : sqlDataReader.GetString(1);
                clanData.ChannelIDs.CarryOverChannelID =
                    (sqlDataReader.GetValue(2) == DBNull.Value) ? null : sqlDataReader.GetString(2);
                clanData.ChannelIDs.TaskKillChannelID =
                    (sqlDataReader.GetValue(3) == DBNull.Value) ? null : sqlDataReader.GetString(3);
                clanData.ChannelIDs.DeclarationChannelID =
                    (sqlDataReader.GetValue(4) == DBNull.Value) ? null : sqlDataReader.GetString(4);
                clanData.ChannelIDs.ReservationChannelID =
                    (sqlDataReader.GetValue(5) == DBNull.Value) ? null : sqlDataReader.GetString(5);
                clanData.ChannelIDs.TimeLineConversionChannelID =
                    (sqlDataReader.GetValue(6) == DBNull.Value) ? null : sqlDataReader.GetString(6);
            }
            sqlDataReader.Close();

            return clanData;
        }

    }
}
