using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.MySQL;
using System.Threading.Tasks;
using System.Linq;

namespace PriconneBotConsoleApp.Script
{
    class BattleReservation
    {
        private ClanData m_userClanData;
        private const int MinBossNumber = 1;
        private const int MaxBossNumber = 5;

        private SocketUserMessage m_userMessage;
        
        public BattleReservation(ClanData userClanData, SocketUserMessage message)
        {
            m_userClanData = userClanData;
            m_userMessage = message;
        }

        async public Task RunReservationCommand()
        {
            var messageContents = m_userMessage.Content;
            if (messageContents.StartsWith("!start"))
            {
                Console.WriteLine("OK");
            }
            else if (messageContents.StartsWith("予約"))
            {
                if (messageContents == "予約" || messageContents == "予約確認")
                {
                    Console.WriteLine("予約確認");
                    return;
                }
                var reservationData = MessageToReservationData();
                if (reservationData is null)
                {
                    await FailedToRegisterMessage();
                    return;
                }
                RegisterReservationData(reservationData);

            }

        }

        public void SendSystemMessage()
        {
            Console.WriteLine("test");
        }

        public void UpdateSystemMessage()
        {

        }

        public void RegisterReservation()
        {

        }

        /// <summary>
        /// 受信した予約データを解析して保存する
        /// 例 : 「予約 35 1 1200万程度」
        /// 「予約 周回 ボス メモ(任意)」
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private ReservationData MessageToReservationData()
        {
            var massageContent = ZenToHan(m_userMessage.Content);
            
            var splitMessageContent =
                massageContent.Split(new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if ((splitMessageContent.Length >= 3 && splitMessageContent.Length <= 4) == false )
            { 
                return null;
            }
            //if ((Regex.IsMatch(splitMessageContent[1], @"\d+") && Regex.IsMatch(splitMessageContent[2], @"[1-5]")) == false)
            //{ return reservationData; }

            if (!(int.TryParse(splitMessageContent[1], out int bossNumber)
                && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber))
            {
                return null;
            }
            if (!(int.TryParse(splitMessageContent[2], out int battleLap) 
                && battleLap > 0))
            {
                return null;
            }

            var serverID = m_userClanData.ServerID;
            var clanRoleID = m_userClanData.ClanRoleID;
            var userID = m_userMessage.Author.Id.ToString();
            string commentData = null;

            if (splitMessageContent.Length == 4)
            {
                commentData = splitMessageContent[3];
            }


            return new ReservationData(serverID, clanRoleID, userID, bossNumber, battleLap, commentData);
        }

        private void RegisterReservationData(ReservationData reservationData)
        {
            using (var mySQLReservationController = new MySQLReservationController())
            {
                var allSqlReservationData =  mySQLReservationController.LoadReservationData(
                    m_userClanData, reservationData.UserID);

                var sqlReservationData = allSqlReservationData
                    .Where(x => x.BossNumber == reservationData.BossNumber )
                    .Where(y => y.BattleLaps == reservationData.BattleLaps );

                if (sqlReservationData.Count() != 0)
                {
                    mySQLReservationController.UpdateReservationData(reservationData);
                }
                else
                {
                    mySQLReservationController.CreateReservationData(reservationData);
                }

            }
        }

        private string DeleteUserReservationData()
        {
            var resultMessage = "";

            return resultMessage;
        }

        private string CreateMessage()
        {

            var hoge = "";
            return hoge;
        }

        async private Task FailedToRegisterMessage()
        {
            var textMessage = "予約に失敗しました。";
            await m_userMessage.Channel.SendMessageAsync(textMessage);
            return ;
        }

        private string ZenToHan(string textData)
        {
            var convertText = textData;
            convertText = Regex.Replace(convertText, "　", p => ((char)(p.Value[0] - '　' + ' ')).ToString());
            convertText = Regex.Replace(convertText, "[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString());
            convertText = Regex.Replace(convertText, "[ａ-ｚ]", p => ((char)(p.Value[0] - 'ａ' + 'a')).ToString());
            convertText = Regex.Replace(convertText, "[Ａ-Ｚ]", p => ((char)(p.Value[0] - 'Ａ' + 'A')).ToString());
            return convertText;
        }
    }
}
