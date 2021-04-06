using System;
using System.Collections.Generic;
using System.Text;

using MySql.Data.MySqlClient;

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
    }
}
