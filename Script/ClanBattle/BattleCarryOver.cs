using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Extension;
using PriconneBotConsoleApp.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PriconneBotConsoleApp.Define;
using PriconneBotConsoleApp.DataType;
using Discord;

namespace PriconneBotConsoleApp.Script
{
    public class BattleCarryOver
    {
        private readonly ClanData m_ClanData;
        private readonly SocketUserMessage m_UserMessage;
        private readonly SocketRole m_ClanRole;
        private readonly SocketGuild m_Guild;

        public BattleCarryOver(ClanData clanData, SocketUserMessage userMessage)
        {
            m_ClanData = clanData;
            m_UserMessage = userMessage;
            m_Guild = (userMessage.Channel as SocketTextChannel)?.Guild;
            m_ClanRole = m_Guild?.GetRole(clanData.ClanRoleID);
        }

        public async Task RunByMessage()
        {
            var messageContent = m_UserMessage.Content;

            if (messageContent.StartsWith("!"))
            {
                if (messageContent.StartsWith("!init"))
                {
                    InitAllData();
                } 
                else if (messageContent.StartsWith("!rm"))
                {

                }

            }
            else
            {
                UpdateCarryOverData();
            }
        }

        /// <summary>
        /// 個人の持ち越し所持・消化報告
        /// </summary>
        private void UpdateCarryOverData()
        {
            var minCommandLength = 1;
            var splitMessage = m_UserMessage.Content.ZenToHan().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var result = false;

            if (splitMessage.Length < minCommandLength)
            {
                return;
            }

            // TODO : 持ち越し番号をEnum化
            if (ConversionAttackNumber.StringToAttackNumber(splitMessage.First()) == 99)
            {
                var userCarryOverData = CommandToCarryOverData(splitMessage);

                if (userCarryOverData == null)
                {
                    return;
                }

                var userID = m_UserMessage.Author.Id;
                var playerData = DatabasePlayerDataController.LoadPlayerData(m_ClanRole, userID);

                if (playerData == null)
                {
                    return;
                }

                var databaseCarryOverList = DatabaseCarryOverController.GetCarryOverData(playerData);
                var databaseCarryOverData = databaseCarryOverList.FirstOrDefault(x => x.BossNumber == userCarryOverData.BossNumber && x.RemainTime == userCarryOverData.RemainTime);

                if (databaseCarryOverData == null && databaseCarryOverList.Count() < Common.MaxCarryOverNumber)
                {
                    result = DatabaseCarryOverController.CreateCarryOverData(userCarryOverData);
                }
                else if (databaseCarryOverData != null)
                {
                    userCarryOverData.CarryOverID = databaseCarryOverData.CarryOverID;
                    result = DatabaseCarryOverController.UpdateCarryOverData(userCarryOverData);
                }
            }
            // TODO:ここの消化記述を別にまとめたい
            else if(splitMessage.First() == "消化")
            {
                var playerData = DatabasePlayerDataController.LoadPlayerData(m_ClanRole, m_UserMessage.Author.Id);

                if (splitMessage.Length > 1 && byte.TryParse(splitMessage[1], out byte deleteNumber))
                {
                    result = DeleteCarryOverData(playerData, deleteNumber);
                }
                else
                {
                    result = DeleteCarryOverData(playerData);
                }
            }

            if (result)
            {
                Task.Run(() => m_UserMessage.AddReactionAsync(new Emoji(EnumMapper.I.GetString(ReactionType.Success))));
            }
        }

        private void UpdateOtherPlayerData()
        {
            var minCommandLength = 1;
            ulong userID = 0;
            byte deleteNumber = 0;
            var splitMessage = m_UserMessage.Content.ZenToHan().Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (splitMessage.Length > minCommandLength)
            {
                userID = m_UserMessage.MentionedUsers.FirstOrDefault().Id;

                if (splitMessage.Length > minCommandLength + 1 && byte.TryParse(splitMessage[2], out var number))
                {
                    deleteNumber = number;
                }
            }
            else
            {
                userID = m_UserMessage.Author.Id;

                if (splitMessage.Length > minCommandLength && byte.TryParse(splitMessage[1], out var number))
                {
                    deleteNumber = number;
                }
            }

            var playerData = DatabasePlayerDataController.LoadPlayerData(m_ClanRole, userID);

            if (DeleteCarryOverData(playerData, deleteNumber))
            {
                Task.Run(() => m_UserMessage.AddReactionAsync(new Emoji(EnumMapper.I.GetString(ReactionType.Success))));
            }
        }

        private bool DeleteCarryOverData(PlayerData playerData, byte deleteNumber = 0)
        {
            var carryOverList = DatabaseCarryOverController.GetCarryOverData(playerData)
                .OrderBy(x => x.DateTime).ToArray();

            if (carryOverList.Length == 0)
            {
                return false;
            }

            var result = false;
            
            if (deleteNumber > 0 && deleteNumber <= carryOverList.Length)
            {
                result = DatabaseCarryOverController.DeleteCarryOverData(carryOverList[deleteNumber - 1]);
            }
            else
            {
                result = DatabaseCarryOverController.DeleteCarryOverData(carryOverList.First());
            }

            return result;
        }

        private void InitAllData()
        {
            var carryOverList = DatabaseCarryOverController.GetCarryOverData(m_ClanData);

            if (carryOverList.Count() == 0)
            {
                return;
            }

            DatabaseCarryOverController.DeleteCarryOverData(carryOverList);
            Task.Run(() => m_UserMessage.AddReactionAsync(new Emoji(EnumMapper.I.GetString(ReactionType.Success))));
        }

        private CarryOverData CommandToCarryOverData(string[] messageData)
        {
            var bossNumberColumn = 1;
            var remainTimeColumn = 2;

            if (!byte.TryParse(messageData[bossNumberColumn], out var bossNumber) || !byte.TryParse(messageData[remainTimeColumn], out var remainTime)
                || bossNumber < Common.MinBossNumber || bossNumber > Common.MaxBossNumber
                || remainTime < Common.MinBattleTime || remainTime > Common.MaxBattleTime)
            {
                return null;
            }

            var playerID = DatabasePlayerDataController.LoadPlayerData(m_ClanRole, m_UserMessage.Author.Id).PlayerID;

            return new CarryOverData()
            {
                BossNumber = bossNumber,
                RemainTime = remainTime,
                CommentData = string.Join(" ", messageData.Skip(3)),
                PlayerID = playerID,
            };
        }

        // 持ち越しを表示するUIを考える
    }
}
