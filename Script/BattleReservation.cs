using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using PriconneBotConsoleApp.DataTypes;


namespace PriconneBotConsoleApp.Script
{
    class BattleReservation
    {
        private ClanData m_userClanData;
        public BattleReservation(ClanData userClanData)
        {
            m_userClanData = userClanData;
        }

        public void RunReservationCommand(SocketUserMessage message)
        {
            var messageContents = message.Content;
            var reservationData = new ReservationData();
            if (messageContents.StartsWith("!start"))
            {
                Console.WriteLine("OK");
            }
            else if (messageContents.StartsWith("予約"))
            {
                reservationData = MessageToReservationData(message);
                Console.WriteLine(reservationData.DataReady.ToString());
            }

        }

        public void SendSystemMessage(SocketUserMessage message)
        {
            Console.WriteLine("test");
        }

        public void UpdateSystemMessage(SocketUserMessage message)
        {

        }

        public void RegisterReservation(SocketUserMessage message)
        {

        }

        /// <summary>
        /// 受信した予約データを解析して保存する
        /// 例 : 「予約 35 1 1200万程度」
        /// 「予約 周回 ボス メモ(任意)」
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private ReservationData MessageToReservationData(SocketUserMessage message)
        {
            var massageContent = ZenToHan(message.Content);
            var reservationData = new ReservationData();
            var splitMessageContent =
                massageContent.Split(new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if ((splitMessageContent.Length >=3 && splitMessageContent.Length <= 4) == false ){ return reservationData; }
            if ((Regex.IsMatch(splitMessageContent[1], @"\d+") && Regex.IsMatch(splitMessageContent[2], @"[1-5]")) == false)
            { return reservationData; }

            reservationData.ServerID = m_userClanData.ServerID;
            reservationData.ClanRoleID = m_userClanData.ClanRoleID;
            reservationData.UserID = message.Author.Id.ToString();
            reservationData.BossNumber = int.Parse( splitMessageContent[2]);
            reservationData.BattleLaps = int.Parse(splitMessageContent[1]);

            if (splitMessageContent.Length == 4)
            {
                reservationData.CommentData = splitMessageContent[3];
            }
            
            reservationData.DataReady = true;

            return reservationData;
        }

        private string DeleteUserReservationData(SocketUserMessage message)
        {
            var resultMessage = "";

            return resultMessage;
        }

        private string CreateMessage()
        {

            var hoge = "";
            return hoge;
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
