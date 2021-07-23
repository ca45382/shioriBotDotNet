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

        private class PlayerInfo
        {
            public readonly ulong PlayerID;
            public readonly string PlayerGuildName;
            public readonly CarryOverData[] CarryOverArray;

            public PlayerInfo(ulong playerID, string playerGuildName, CarryOverData[] carryOverData = null)
            {
                PlayerID = playerID;
                PlayerGuildName = playerGuildName;
                CarryOverArray = carryOverData ?? Array.Empty<CarryOverData>();
            }

            public string GetCarryOverString()
            {
                if (CarryOverArray.Length == 0)
                {
                    return string.Empty;
                }

                var stringData = string.Join(
                    '\n',
                    CarryOverArray.Select(x => $"\t- {x.RemainTime}秒 {x.BossNumber}ボス {x.CommentData}").ToArray()
                    );

                return $"- {PlayerGuildName} {CarryOverArray.Length}つ\n{stringData}";
            }
        }

        public BattleCarryOver(ClanData clanData, SocketUserMessage userMessage)
        {
            m_ClanData = clanData;
            m_UserMessage = userMessage;
            var guild = (userMessage.Channel as SocketTextChannel)?.Guild;
            m_ClanRole = guild?.GetRole(clanData.ClanRoleID);
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
                    UpdateOtherPlayerData();
                }
                else if (messageContent.StartsWith("!list"))
                {
                    await SendClanCarryOverList();
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
            const int minCommandLength = 1;
            // TODO : " "を定数化する。
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

                var databaseCarryOverList = DatabaseCarryOverController.GetCarryOverData(playerData).ToArray();
                var databaseCarryOverData = databaseCarryOverList.FirstOrDefault(x => x.BossNumber == userCarryOverData.BossNumber && x.RemainTime == userCarryOverData.RemainTime);

                if (databaseCarryOverData == null && databaseCarryOverList.Length < CommonDefine.MaxCarryOverNumber)
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
            else if (splitMessage.First() == "消化")
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
            const int minCommandLength = 1;
            ulong userID = 0;
            byte deleteNumber = 0;
            // TODO : " "を定数化する。
            var splitMessage = m_UserMessage.Content.ZenToHan().Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (splitMessage.Length > minCommandLength)
            {
                userID = m_UserMessage.MentionedUsers.FirstOrDefault()?.Id ?? 0;

                if (splitMessage.Length > minCommandLength + 1 && byte.TryParse(splitMessage[2], out var number))
                {
                    deleteNumber = number;
                }
            }

            if (userID == 0)
            {
                return;
            }

            var playerData = DatabasePlayerDataController.LoadPlayerData(m_ClanRole, userID);

            if (DeleteCarryOverData(playerData, deleteNumber))
            {
                Task.Run(() => m_UserMessage.AddReactionAsync(new Emoji(EnumMapper.I.GetString(ReactionType.Success))));
            }
        }

        private async Task SendClanCarryOverList()
        {
            var clanCarryOverEmbed = CreateEmbedData();
            await m_UserMessage.Channel.SendMessageAsync(embed: clanCarryOverEmbed);
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

            if (!carryOverList.Any())
            {
                return;
            }

            DatabaseCarryOverController.DeleteCarryOverData(carryOverList);
            Task.Run(() => m_UserMessage.AddReactionAsync(new Emoji(EnumMapper.I.GetString(ReactionType.Success))));
        }

        private CarryOverData CommandToCarryOverData(string[] messageData)
        {
            const int bossNumberColumn = 1;
            const int remainTimeColumn = 2;

            if (!byte.TryParse(messageData[bossNumberColumn], out var bossNumber) || !byte.TryParse(messageData[remainTimeColumn], out var remainTime)
                || bossNumber < CommonDefine.MinBossNumber || bossNumber > CommonDefine.MaxBossNumber
                || remainTime < CommonDefine.MinBattleTime || remainTime > CommonDefine.MaxBattleTime)
            {
                return null;
            }

            var playerID = DatabasePlayerDataController.LoadPlayerData(m_ClanRole, m_UserMessage.Author.Id).PlayerID;

            return new CarryOverData()
            {
                BossNumber = bossNumber,
                RemainTime = remainTime,
                // TODO : " "を定数化する。
                CommentData = string.Join(" ", messageData.Skip(3)),
                PlayerID = playerID,
            };
        }

        // 持ち越しを表示するUIを考える
        private Embed CreateEmbedData()
        {
            var carryOverArray = DatabaseCarryOverController.GetCarryOverData(m_ClanData)
                .OrderBy(x => x.DateTime);
            var playerArray = DatabasePlayerDataController.LoadPlayerData(m_ClanData);
            List<PlayerInfo> carryOverStringList = new();

            foreach (var playerData in playerArray)
            {
                var userCarryOverList = carryOverArray.Where(x => x.PlayerID == playerData.PlayerID).ToArray();

                if (!userCarryOverList.Any())
                {
                    continue;
                }

                carryOverStringList.Add(new PlayerInfo(playerData.PlayerID, playerData.GuildUserName, userCarryOverList));
            }

            // TODO : 改行文字も定数化
            var stringData = string.Join('\n', carryOverStringList.Select(x => x.GetCarryOverString()));

            EmbedBuilder embedBuilder = new();
            EmbedFieldBuilder embedFieldBuilder = new();

            embedFieldBuilder.Name = "持ち越し所持者";
            embedFieldBuilder.Value = stringData == "" ? "持ち越しなし" : "```python\n" + stringData + "\n```";
            embedBuilder.AddField(embedFieldBuilder);

            embedBuilder.Title = $"{DateTime.Now:t}の残持ち越し";

            return embedBuilder.Build();
        }
    }
}
