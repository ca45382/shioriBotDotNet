using System;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.WebSocket;
using Discord.Rest;

using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.MySQL;
using System.Threading.Tasks;
using System.Linq;

namespace PriconneBotConsoleApp.Script
{
    class BattleDeclaration:BaseClass
    {
        private const int MinBossNumber = 1;
        private const int MaxBossNumber = 5;

        private ClanData m_userClanData;
        private SocketRole m_userRole;
        private SocketUserMessage m_userMessage;
        private SocketReaction m_userReaction;

        public BattleDeclaration(ClanData userClanData, SocketUserMessage message)
        {
            m_userClanData = userClanData;
            m_userMessage = message;

            var socketGuildChannel = message.Channel as SocketGuildChannel;
            m_userRole = socketGuildChannel.Guild.GetRole(
                ulong.Parse(m_userClanData.ClanRoleID)
            );
        }

        public BattleDeclaration(ClanData userClanData, SocketReaction reaction)
        {
            m_userClanData = userClanData;
            m_userReaction = reaction;

            var socketGuildChannel = m_userReaction.Channel as SocketGuildChannel;
            m_userRole = socketGuildChannel.Guild.GetRole(
                ulong.Parse(m_userClanData.ClanRoleID)
            );
        }

        async public Task RunDeclarationCommand()
        {
            var userMessage = m_userMessage;
            if (userMessage == null) return;
            var messageContents = userMessage.Content;

            if (messageContents.StartsWith("!call"))
            {
                await DeclarationCallCommand();
            }
            return;
        }

        async private Task<bool> DeclarationCallCommand()
        {
            var userMessage = m_userMessage;
            var userClanData = m_userClanData;

            var massageContent = ZenToHan(userMessage.Content);

            var splitMessageContent = massageContent.Split(
                    new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if (splitMessageContent.Length != 3) return false;

            if (!(int.TryParse(splitMessageContent[1], out int battleLap)
                && battleLap > 0))
            {
                return false;
            }
            if (!(int.TryParse(splitMessageContent[2], out int bossNumber)
                && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber))
            {
                return false;
            }

            userClanData.BattleLap = battleLap;
            userClanData.BossNumber = bossNumber;

            var result = new MySQLClanDataController().UpdateClanData(userClanData);

            if (result == false) return false;

            m_userClanData = userClanData;

            result = await SendDeclarationBotMessage();

            return result;
        }

        async private Task<bool> SendDeclarationBotMessage()
        {
            var userClanData = m_userClanData;
            var userRole = m_userRole;

            var embed = CreateDeclarationDataEmbed(userClanData);

            var declarationChannel = userRole.Guild.GetTextChannel(
                ulong.Parse(userClanData.ChannelIDs.DeclarationChannelID));

            var sendedMessage = await declarationChannel.SendMessageAsync(embed: embed);

            if (sendedMessage == null) return false;

            await AttacheDefaultReaction(sendedMessage);

            var result = new MySQLDeclarationController().UpdateDeclarationMessageID(
                userClanData, sendedMessage.Id.ToString());

            return result;
        }

        async public Task<bool> UpdateDeclarationBotMessage()
        {
            var userClanData = m_userClanData;
            var userRole = m_userRole;

            var declarationMessageID = userClanData.MessageIDs.DeclarationMessageID;
            if (declarationMessageID == null) return false;

            var guildChannel = userRole.Guild.GetChannel(
                ulong.Parse(userClanData.ChannelIDs.DeclarationChannelID)) as SocketTextChannel;

            SocketUserMessage declarationBotMessage =
                guildChannel.GetCachedMessage(ulong.Parse(declarationMessageID)) 
                as SocketUserMessage;

            if (declarationBotMessage == null)
            {
                var message = await guildChannel.GetMessageAsync(
                    ulong.Parse(declarationMessageID));
                if (message == null) return false;

                await guildChannel.DeleteMessageAsync(message);
                await SendDeclarationBotMessage();

            }

            var embed = CreateDeclarationDataEmbed(userClanData);
            await declarationBotMessage.ModifyAsync(msg => msg.Embed = embed);

            return true;

        }

        /// <summary>
        /// 凸宣言した時のコード
        /// </summary>
        /// <returns></returns>
        private bool UserRegistorDeclareCommand()
        {
            var userReaction = m_userReaction;
            var declarationData = UserToDeclareData(userReaction.UserId.ToString());
            var result = new MySQLDeclarationController()
                .CreateDeclarationData(declarationData);

            return result;
        }

        /// <summary>
        /// 本戦が終了した際のコード
        /// </summary>
        /// <returns></returns>
        private bool UserFinishBattleCommand()
        {
            var userRole = m_userRole;
            var userClanData = m_userClanData;
            var userReaction = m_userReaction;


            var declarationData = UserToDeclareData(userReaction.UserId.ToString());  
            var result = new MySQLDeclarationController()
                .UpdateDeclarationData(declarationData);

            var playerData = new MySQLPlayerDataController().LoadPlayerData(
                userRole.Guild.Id.ToString(), userReaction.UserId.ToString());

            var mySQLReservationController = new MySQLReservationController();
            var reservationData = mySQLReservationController.LoadReservationData(playerData);
            var finishReservationData = reservationData
                .Where(d => d.BattleLap == userClanData.BattleLap)
                .Where(d => d.BossNumber == userClanData.BossNumber)
                .FirstOrDefault();

            if (finishReservationData != null)
            {
                mySQLReservationController.DeleteReservationData(finishReservationData);
            }

            return result;

        }

        /// <summary>
        /// ユーザーがバトルをキャンセルした際のコード
        /// </summary>
        /// <returns></returns>
        private bool UserDeleteBattleData()
        {
            var userReaction = m_userReaction;

            return true;
        }

        private bool NextBossCommand()
        {

            return true;
        }

        private DeclarationData UserToDeclareData(string userID)
        {
            var userClanData = m_userClanData;
            var userRole = m_userRole;

            var playerData = new MySQLPlayerDataController().LoadPlayerData(
                userRole.Guild.Id.ToString(), userID);

            return new DeclarationData()
            {
                PlayerID = playerData.PlayerID,
                BattleLap = userClanData.BattleLap,
                BossNumber = userClanData.BossNumber,
                FinishFlag = true
            };
        }

        async private Task AttacheDefaultReaction(IMessage message)
        {

            string[] emojiData = { "⚔️", "✅", "🏁", "❌" };
            var emojiMatrix = Enumerable
                .Range(0, 6)
                .Select((x) => new Emoji(emojiData[x]))
                .ToList();

            foreach (var emoji in emojiMatrix)
            {
                await message.AddReactionAsync(emoji);
            }
            return;

        }

        private Embed CreateDeclarationDataEmbed(ClanData clanData)
        {
            var reservationDataList =
                new MySQLReservationController().LoadBossLapReservationData(clanData);
            var declarationDataList =
                new MySQLDeclarationController().LoadDeclarationData(clanData);

            var bossNumber = clanData.BossNumber;
            var battleLap = clanData.BattleLap;

            var updateTime = DateTime.Now;
            var updateTimeString = updateTime.ToString("T");

            var reservationNameList = reservationDataList
                .OrderBy(d => d.DateTime)
                .Select(d => d.PlayerData.GuildUserName)
                .ToList();

            var reservationListMessage = NameListToMessageData(reservationNameList);
            var reservationPlayerCount = reservationNameList.Count();
            if (reservationPlayerCount == 0)
            {
                reservationListMessage = "予約なし";
            }

            var finishNameList = declarationDataList
                .Where(d => d.FinishFlag == true)
                .OrderBy(d => d.DateTime)
                .Select(d => d.PlayerData.GuildUserName)
                .ToList();

            var finishListMessage = NameListToMessageData(finishNameList);
            var finishPlayerCount = finishNameList.Count();
            if (finishPlayerCount == 0)
            {
                finishListMessage = "完了者なし";
            }

            var nowBattleNameList = declarationDataList
                .Where(d => d.FinishFlag == false)
                .OrderBy(d => d.DateTime)
                .Select(d => d.PlayerData.GuildUserName)
                .ToList();

            var nowBattleListMessage = NameListToMessageData(nowBattleNameList);
            var nowBattlePlayerCount = nowBattleNameList.Count();
            if (nowBattlePlayerCount == 0)
            {
                nowBattleListMessage = "宣言なし";
            }

            var embedBuild = new EmbedBuilder
            {
                Title = $"凸宣言({battleLap, 2}周目{bossNumber,1}ボス)"
            };

            embedBuild.AddField(new EmbedFieldBuilder()
            {
                IsInline = true,
                Name = $"本戦宣言中({nowBattlePlayerCount}人)",
                Value = nowBattleListMessage
            });

            embedBuild.AddField(new EmbedFieldBuilder()
            {
                IsInline = true,
                Name = $"予約中({reservationPlayerCount}人)",
                Value = reservationListMessage
            });

            embedBuild.AddField(new EmbedFieldBuilder()
            {
                IsInline = false,
                Name = $"本戦完了({finishPlayerCount}人)",
                Value = finishListMessage
            });

            embedBuild.Color = Color.Red;
            embedBuild.Footer = new EmbedFooterBuilder()
            {
                Text = $"最終更新時刻:{updateTimeString}"
            };

            var embed = embedBuild.Build();

            return embed;
        }

        private string NameListToMessageData(List<string> nameDataSet)
        {
            if (nameDataSet.Count() == 0) return "";

            string messageData = "";
            var nameCount = 0;

            foreach (var nameData in nameDataSet)
            {
                nameCount += 1;
                messageData += $"{nameCount,2}. {nameData}\n";
            }

            return messageData;
        }
    }
}
