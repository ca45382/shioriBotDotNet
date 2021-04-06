using System;
using System.Collections.Generic;
using System.Text;

using MySql.Data.MySqlClient;
using PriconneBotConsoleApp.DataTypes;



namespace PriconneBotConsoleApp.MySQL
{
    class MySQLPlayerDataController : MySQLConnector
    {
        /// <summary>
        /// プレイヤーデータを読み込む
        /// </summary>
        /// <param name="guildID"></param>
        /// <returns> PlayerInfoのList</returns>
        public List<PlayerData> LoadPlayerData(string guildID)
        {
            var commandString = 
                "SELECT " +
                "server_id, clan_role_id, user_id, name " +
                "FROM player_data " +
                $"WHERE server_id = {guildID}";
            var command = new MySqlCommand();
            var result = new List<PlayerData>();

            command.Connection = m_mySQLConnection;
            command.CommandText = commandString;
            var sqlDataReader= command.ExecuteReader();
            while (sqlDataReader.Read())
            {
                result.Add(new PlayerData()
                {
                    ServerID = sqlDataReader.GetString(0),
                    ClanRoleID = sqlDataReader.GetString(1),
                    UserID = sqlDataReader.GetString(2),
                    GuildUserName = sqlDataReader.GetString(3)
                });
            }
            sqlDataReader.Close();

            return result; 
        }

        public void CreatePlayerData(List<PlayerData> playersData)
        {
            var transaction = m_mySQLConnection.BeginTransaction();
            foreach(PlayerData player in playersData)
            {
                InsertData(player);
            }
            transaction.Commit();
        }

        public void CreatePlayerData(PlayerData playerData)
        {
            var transaction = m_mySQLConnection.BeginTransaction();
            InsertData(playerData);
            transaction.Commit();
        }

        public void UpdatePlayerData(List<PlayerData> playersData)
        {
            var transaction = m_mySQLConnection.BeginTransaction();
            foreach (PlayerData player in playersData)
            {
                UpdateData(player);
            }
            transaction.Commit();
        }

        public void UpdatePlayerData(PlayerData playerData)
        {
            var transaction = m_mySQLConnection.BeginTransaction();
            UpdateData(playerData);
            transaction.Commit();
        }

        public void DeletePlayerData(List<PlayerData> playersData)
        {
            var transaction = m_mySQLConnection.BeginTransaction();
            foreach (PlayerData player in playersData)
            {
                DeleteData(player);
            }
            transaction.Commit();
        }

        public void DeletePLayerData(PlayerData playerData)
        {
            var transaction = m_mySQLConnection.BeginTransaction();
            DeleteData(playerData);
            transaction.Commit();
        }

        private int InsertData(PlayerData playerData)
        {
            var commandString =
                "INSERT " +
                "INTO player_data " +
                "VALUES (@serverID, @clanRoleID, @userID, @guildName)";

            var mySQLCommand = new MySqlCommand(
                    commandString, m_mySQLConnection
                    );
            mySQLCommand.Parameters.Add(
                new MySqlParameter("serverID", playerData.ServerID)
                );
            mySQLCommand.Parameters.Add(
                new MySqlParameter("clanRoleID", playerData.ClanRoleID)
                );
            mySQLCommand.Parameters.Add(
                new MySqlParameter("userID", playerData.UserID)
                );
            mySQLCommand.Parameters.Add(
                new MySqlParameter("guildName", playerData.GuildUserName)
                );

            var result = mySQLCommand.ExecuteNonQuery();
            return result;
        }

        private int UpdateData(PlayerData playerData)
        {
            var commandString =
                "UPDATE " +
                "player_data " +
                "SET name = @guildName " +
                "WHERE server_id = @serverID " +
                "AND clan_role_id = @clanRoleID " +
                "AND user_id = @userID";

            var mySQLCommand = new MySqlCommand(
                    commandString, m_mySQLConnection
                    );
            mySQLCommand.Parameters.Add(
                new MySqlParameter("serverID", playerData.ServerID)
                );
            mySQLCommand.Parameters.Add(
                new MySqlParameter("clanRoleID", playerData.ClanRoleID)
                );
            mySQLCommand.Parameters.Add(
                new MySqlParameter("userID", playerData.UserID)
                );
            mySQLCommand.Parameters.Add(
                new MySqlParameter("guildName", playerData.GuildUserName)
                );

            var result = mySQLCommand.ExecuteNonQuery();
            return result;
        }

        private int DeleteData(PlayerData playerData)
        {
            var commandString =
                "DELETE " +
                "FROM player_data " +
                "WHERE server_id = @serverID " +
                "AND clan_role_id = @clanRoleID " +
                "AND user_id = @userID";

            var mySQLCommand = new MySqlCommand(
                    commandString, m_mySQLConnection
                    );
            mySQLCommand.Parameters.Add(
                new MySqlParameter("serverID", playerData.ServerID)
                );
            mySQLCommand.Parameters.Add(
                new MySqlParameter("clanRoleID", playerData.ClanRoleID)
                );
            mySQLCommand.Parameters.Add(
                new MySqlParameter("userID", playerData.UserID)
                );

            var result = mySQLCommand.ExecuteNonQuery();
            return result;
        }

    }
}
