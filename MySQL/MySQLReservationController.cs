using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

using MySql.Data.MySqlClient;
using PriconneBotConsoleApp.DataTypes;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLReservationController:MySQLConnectorOld
    {
        /// <summary>
        /// クラン内すべての予約情報を取得する。
        /// </summary>
        /// <param name="clanData"></param>
        /// <returns></returns>
        public List<ReservationData> LoadReservationData(ClanData clanData)
        {
            var commandString =
                "SELECT server_id, clan_role_id, user_id, " +
                "reserve_data.boss_num, reserve_data.battle_lap, reserve_data.comment_data " +
                "FROM reserve_data " +
                "INNER JOIN player_data ON reserve_data.player_id = player_data.player_id " +
                "INNER JOIN clan_info ON player_data.clan_id = clan_info.clan_id " +
                "WHERE server_id = @serverID AND clan_role_id = @clanRoleID " +
                "AND delete_flag = 0 " +
                "ORDER BY battle_lap, boss_num, date_time";

            var mySqlCommand = new MySqlCommand(
                commandString, m_mySQLConnection
                );

            mySqlCommand.Parameters.Add(
                new MySqlParameter("serverID", clanData.ServerID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("clanRoleID", clanData.ClanRoleID)
            );

            var sqlDataReader = mySqlCommand.ExecuteReader();
            var result = new List<ReservationData>();

            while (sqlDataReader.Read())
            {
                var userReservationData = new ReservationData
                (
                    (sqlDataReader.GetValue(0) == DBNull.Value) ? null : sqlDataReader.GetString(0),
                    (sqlDataReader.GetValue(1) == DBNull.Value) ? null : sqlDataReader.GetString(1),
                    (sqlDataReader.GetValue(2) == DBNull.Value) ? null : sqlDataReader.GetString(2),
                    (sqlDataReader.GetValue(3) == DBNull.Value) ? 0 : sqlDataReader.GetInt32(3),
                    (sqlDataReader.GetValue(4) == DBNull.Value) ? 0 : sqlDataReader.GetInt32(4),
                    (sqlDataReader.GetValue(5) == DBNull.Value) ? null : sqlDataReader.GetString(5)
                );

                result.Add(userReservationData);
            }
            sqlDataReader.Close();

            return result;
        }

        /// <summary>
        /// ユーザーの予約情報を取得する。
        /// </summary>
        /// <param name="clanData"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<ReservationData> LoadReservationData(ClanData clanData, string userID)
        {
            var commandString =
                "SELECT server_id, clan_role_id, user_id, " +
                "reserve_data.boss_num, reserve_data.battle_lap, reserve_data.comment_data " +
                "FROM reserve_data " +
                "INNER JOIN player_data ON reserve_data.player_id = player_data.player_id " +
                "INNER JOIN clan_info ON player_data.clan_id = clan_info.clan_id " +
                "WHERE server_id = @serverID AND clan_role_id = @clanRoleID " +
                "AND user_id = @userID AND delete_flag = 0 " +
                "ORDER BY battle_lap, boss_num";

            var mySqlCommand = new MySqlCommand(
                commandString, m_mySQLConnection
                );

            mySqlCommand.Parameters.Add(
                new MySqlParameter("serverID", clanData.ServerID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("clanRoleID", clanData.ClanRoleID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("userID", userID)
            );

            var sqlDataReader = mySqlCommand.ExecuteReader();
            var result = new List<ReservationData>();

            while (sqlDataReader.Read())
            {
                var oneReservationData = new ReservationData(
                    (sqlDataReader.GetValue(0) == DBNull.Value) ? null : sqlDataReader.GetString(0),
                    (sqlDataReader.GetValue(1) == DBNull.Value) ? null : sqlDataReader.GetString(1),
                    (sqlDataReader.GetValue(2) == DBNull.Value) ? null : sqlDataReader.GetString(2),
                    (sqlDataReader.GetValue(3) == DBNull.Value) ? 0 : sqlDataReader.GetInt32(3),
                    (sqlDataReader.GetValue(4) == DBNull.Value) ? 0 : sqlDataReader.GetInt32(4),
                    (sqlDataReader.GetValue(5) == DBNull.Value) ? null : sqlDataReader.GetString(5)
                );

                result.Add(oneReservationData);
            }
            sqlDataReader.Close();

            return result;

        }


        public void CreateReservationData(ReservationData reservationData)
        {
            var commandString =
                "INSERT INTO reserve_data " +
                "(player_id, boss_num, battle_lap, comment_data ) " +
                "SELECT player_id, @bossNumber, @battleLap, @commentData " +
                "FROM player_data INNER JOIN clan_info ON player_data.clan_id = clan_info.clan_id " +
                "WHERE server_id = @serverID AND clan_role_id = @clanRoleID " +
                "AND user_id = @userID";

            var mySqlCommand = new MySqlCommand(
                commandString, m_mySQLConnection
                );

            var transaction = m_mySQLConnection.BeginTransaction();

            mySqlCommand.Parameters.Add(
                new MySqlParameter("serverID", reservationData.ServerID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("clanRoleID", reservationData.ClanRoleID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("userID", reservationData.UserID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("bossNumber", reservationData.BossNumber)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("battleLap", reservationData.BattleLaps)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("commentData", reservationData.CommentData)
            );

            mySqlCommand.ExecuteNonQuery();

            transaction.Commit();

        }

        public void UpdateReservationData(ReservationData reservationData)
        {
            var commandString =
                "UPDATE reserve_data " +
                "SET comment_data = @commentData " +
                "WHERE boss_num = @bossNumber AND battle_lap = @battleLap " +
                "AND delete_flag = 0 " +
                "AND player_id IN " +
                "(SELECT player_id " +
                "FROM player_data INNER JOIN clan_info ON clan_info.clan_id = player_data.clan_id " +
                "WHERE server_id = @serverID AND clan_role_id = @clanRoleID " +
                "AND user_id = @userID)";

            var mySqlCommand = new MySqlCommand(
                commandString, m_mySQLConnection
                );

            var transaction = m_mySQLConnection.BeginTransaction();

            mySqlCommand.Parameters.Add(
                new MySqlParameter("serverID", reservationData.ServerID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("clanRoleID", reservationData.ClanRoleID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("userID", reservationData.UserID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("bossNumber", reservationData.BossNumber)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("battleLap", reservationData.BattleLaps)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("commentData", reservationData.CommentData)
            );

            mySqlCommand.ExecuteNonQuery();

            transaction.Commit();
        }

        public void DeleteReservationData(ReservationData reservationData)
        {
            var transaction = m_mySQLConnection.BeginTransaction();
            DeleteData(reservationData);
            transaction.Commit();
        }

        public void DeleteReservationData(List<ReservationData> reservationDatas)
        {
            var transaction = m_mySQLConnection.BeginTransaction();
            foreach (var reservationData in reservationDatas)
            {
                DeleteData(reservationData);
            }
            transaction.Commit();

        }

        private void DeleteData(ReservationData reservationData)
        {
            var commandString =
                "UPDATE reserve_data " +
                "SET delete_flag = 1" +
                "WHERE boss_num = @bossNumber AND battle_lap = @battleLap " +
                "AND player_id IN " +
                "(SELECT player_id " +
                "FROM player_data INNER JOIN clan_info ON clan_info.clan_id = player_data.clan_id " +
                "WHERE server_id = @serverID AND clan_role_id = @clanRoleID " +
                "AND user_id = @userID )";

            var mySqlCommand = new MySqlCommand(
               commandString, m_mySQLConnection
               );

            mySqlCommand.Parameters.Add(
                new MySqlParameter("serverID", reservationData.ServerID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("clanRoleID", reservationData.ClanRoleID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("userID", reservationData.UserID)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("bossNumber", reservationData.BossNumber)
            );
            mySqlCommand.Parameters.Add(
                new MySqlParameter("battleLap", reservationData.BattleLaps)
            );

            mySqlCommand.ExecuteNonQuery();
        }
    }
}
