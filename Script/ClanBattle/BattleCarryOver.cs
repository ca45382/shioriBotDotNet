using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Define;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public class BattleCarryOver
    {
        private readonly CommandEventArgs m_CommandEventArgs;

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

        public BattleCarryOver(CommandEventArgs commandEventArgs)
        {
            m_CommandEventArgs = commandEventArgs;
        }

        /// <summary>
        /// 持ち越しを登録するか更新する. 引数は2以上
        /// </summary>
        public void UpdateCarryOverData()
        {
            var playerData = DatabasePlayerDataController
                .LoadPlayerData(m_CommandEventArgs.Role, m_CommandEventArgs.SocketUserMessage.Author.Id);

            if (!byte.TryParse(m_CommandEventArgs.Arguments[0], out var bossNumber)
                || !byte.TryParse(m_CommandEventArgs.Arguments[1], out var remainTime)
                || !CommonDefine.IsValidBossNumber(bossNumber)
                || !CommonDefine.IsValidBattleTime(remainTime)
                || playerData == null)
            {
                return;
            }

            CarryOverData carryOverData = new()
            {
                BossNumber = bossNumber,
                RemainTime = remainTime,
                // TODO : " "を定数化する。
                CommentData = string.Join(" ", m_CommandEventArgs.Arguments.Skip(2)),
                PlayerID = playerData.PlayerID,
            };

            var databaseCarryOverList = DatabaseCarryOverController.GetCarryOverData(playerData).ToArray();
            var databaseCarryOverData = databaseCarryOverList.FirstOrDefault(x => x.BossNumber == carryOverData.BossNumber && x.RemainTime == carryOverData.RemainTime);
            var result = false;

            if (databaseCarryOverData != null)
            {
                carryOverData.CarryOverID = databaseCarryOverData.CarryOverID;
                result = DatabaseCarryOverController.UpdateCarryOverData(carryOverData);
            }
            else if (databaseCarryOverList.Length < CommonDefine.MaxCarryOverNumber)
            {
                result = DatabaseCarryOverController.CreateCarryOverData(carryOverData);
            }

            if (result)
            {
                _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(ReactionType.Success.ToEmoji());
            }
        }

        /// <summary>
        /// 持ち越しデータを削除する時に用いる。引数0か1。
        /// </summary>
        public void DeleteCarryOverData()
        {
            const int defaultDeleteNumber = 1;
            var playerData = DatabasePlayerDataController.LoadPlayerData(m_CommandEventArgs.Role, m_CommandEventArgs.User.Id);

            try
            {
                if (byte.TryParse(m_CommandEventArgs.Arguments.ElementAtOrDefault(0), out var deleteNumber))
                {
                    DeletePlayerCarryOverData(playerData, deleteNumber);
                }
                else
                {
                    DeletePlayerCarryOverData(playerData, defaultDeleteNumber);
                }
            }
            catch
            {
                return;
            }

            _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(ReactionType.Success.ToEmoji());
        }

        /// <summary>
        /// 他人の持越しを削除する。引数は2つ必要とする。
        /// </summary>
        public void DeleteOtherPlayerData()
        {
            var targetUser = MentionUtils.TryParseUser(m_CommandEventArgs.Arguments[0], out var userID)
                    || ulong.TryParse(m_CommandEventArgs.Arguments[0], out userID)
                ? m_CommandEventArgs.Role.Guild.GetUser(userID)
                : throw new ArgumentNullException();

            // コマンドは `!rm @削除対象のユーザー 古い方から何番目か` としている。
            if (!byte.TryParse(m_CommandEventArgs.Arguments[1], out var number)
                || number <= 0
                || CommonDefine.MaxReportNumber < number)
            {
                throw new ArgumentOutOfRangeException();
            }

            var playerData = DatabasePlayerDataController.LoadPlayerData(m_CommandEventArgs.Role, targetUser.Id);

            try
            {
                DeletePlayerCarryOverData(playerData, number);
                _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(ReactionType.Success.ToEmoji());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 持ち越しリストを表示するコマンド。引数0か1。
        /// </summary>
        /// <returns></returns>
        public async Task SendClanCarryOverList()
        {
            var clanCarryOverEmbed = CreateEmbedData();
            await m_CommandEventArgs.Channel.SendMessageAsync(embed: clanCarryOverEmbed);
        }

        private void DeletePlayerCarryOverData(PlayerData playerData, byte deleteNumber)
        {
            var carryOverList = DatabaseCarryOverController.GetCarryOverData(playerData)
                .OrderBy(x => x.DateTime)
                .ToArray();

            if (carryOverList.Length == 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            DatabaseCarryOverController.DeleteCarryOverData(carryOverList[(deleteNumber > 0 && deleteNumber <= carryOverList.Length) ? deleteNumber - 1 : 0]);
        }

        /// <summary>
        /// 全ての持ち越しを削除する。引数は0。
        /// </summary>
        public void InitAllData()
        {
            var carryOverList = DatabaseCarryOverController.GetCarryOverData(m_CommandEventArgs.ClanData);

            if (!carryOverList.Any())
            {
                return;
            }

            DatabaseCarryOverController.DeleteCarryOverData(carryOverList);
            _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(ReactionType.Success.ToEmoji());

            _ = m_CommandEventArgs.Channel.SendTimedMessageAsync(
                TimeDefine.SuccessMessageDisplayTime,
                string.Format(EnumMapper.ToLabel(InfomationType.DeleteAllCarryOverData), TimeDefine.SuccessMessageDisplayTime)
            );
        }

        // 持ち越しを表示するUIを考える
        private Embed CreateEmbedData()
        {
            var carryOverArray = DatabaseCarryOverController.GetCarryOverData(m_CommandEventArgs.ClanData)
                .OrderBy(x => x.DateTime);

            var playerArray = DatabasePlayerDataController.LoadPlayerData(m_CommandEventArgs.ClanData);
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
