﻿using Discord;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.MySQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Script
{
    class BattleDeclaration : BaseClass
    {
        private const int MinBossNumber = 1;
        private const int MaxBossNumber = 5;

        private readonly ClanData m_userClanData;
        private readonly SocketRole m_userRole;
        private readonly SocketUserMessage m_userMessage;
        private readonly SocketReaction m_userReaction;

        private BattleDeclaration(
            ClanData userClanData,
            ISocketMessageChannel channel,
            SocketUserMessage userMessage = null,
            SocketReaction userReaction = null)
        {
            m_userClanData = userClanData;
            m_userRole = (channel as SocketGuildChannel)?.Guild.GetRole(ulong.Parse(m_userClanData.ClanRoleID));
            m_userMessage = userMessage;
            m_userReaction = userReaction;
        }

        public BattleDeclaration(ClanData userClanData, SocketUserMessage message)
            : this(userClanData, message.Channel, userMessage: message)
        {
        }

        public BattleDeclaration(ClanData userClanData, SocketReaction reaction)
            : this(userClanData, reaction.Channel, userReaction: reaction)
        {
        }

        public async Task RunDeclarationCommandByMessage()
        {
            if (m_userMessage != null && m_userMessage.Content.StartsWith("!call"))
            {
                await DeclarationCallCommand();
                await new BattleReservation(m_userClanData, m_userMessage).UpdateSystemMessage();
            }
        }

        public async Task RunDeclarationCommandByReaction()
        {
            switch (m_userReaction.Emote.Name)
            {
                case "⚔️":
                    UserRegistorDeclareCommand();
                    break;

                case "✅":
                    UserFinishBattleCommand();
                    break;

                case "🏁":
                    await NextBossCommand();
                    await new BattleReservation(m_userClanData, m_userReaction).UpdateSystemMessage();
                    return;

                case "❌":
                    UserDeleteBattleData();
                    break;
            }

            await UpdateDeclarationBotMessage();
            await RemoveUserReaction();
            await new BattleReservation(m_userClanData, m_userReaction).UpdateSystemMessage();
        }

        private async Task<bool> DeclarationCallCommand()
        {
            var splitMessageContent = ZenToHan(m_userMessage.Content)
                .Split(new[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if (splitMessageContent.Length != 3
                || !(int.TryParse(splitMessageContent[1], out int battleLap) && battleLap > 0)
                || !(int.TryParse(splitMessageContent[2], out int bossNumber) && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber))
            {
                return false;
            }

            m_userClanData.BattleLap = battleLap;
            m_userClanData.BossNumber = bossNumber;

            if (!new MySQLClanDataController().UpdateClanData(m_userClanData))
            {
                return false;
            }

            return await SendDeclarationBotMessage();
        }

        private async Task<bool> SendDeclarationBotMessage()
        {
            var embed = CreateDeclarationDataEmbed(m_userClanData);
            var content = CreateDeclarationDataMessage(m_userClanData);
            var declarationChannel = m_userRole.Guild.GetTextChannel(ulong.Parse(m_userClanData.ChannelIDs.DeclarationChannelID));
            var sentMessage = await declarationChannel.SendMessageAsync(text: content, embed: embed);

            if (sentMessage == null)
            {
                return false;
            }

            var sentMessageId = sentMessage.Id.ToString();

            var result = new MySQLDeclarationController()
                .UpdateDeclarationMessageID(m_userClanData, sentMessageId);

            await AttacheDefaultReaction(sentMessage);
            m_userClanData.MessageIDs.DeclarationMessageID = sentMessageId;

            return result;
        }

        public async Task<bool> UpdateDeclarationBotMessage()
        {
            var userClanData = m_userClanData;
            var userRole = m_userRole;

            var declarationMessageID = userClanData.MessageIDs.DeclarationMessageID;
            
            if (declarationMessageID == null)
            {
                return false;
            }

            var guildChannel = userRole.Guild.GetChannel(ulong.Parse(userClanData.ChannelIDs.DeclarationChannelID)) as SocketTextChannel;

            if (guildChannel.GetCachedMessage(ulong.Parse(declarationMessageID)) is SocketUserMessage declarationBotMessage)
            {
                var embed = CreateDeclarationDataEmbed(userClanData);
                await declarationBotMessage.ModifyAsync(msg => msg.Embed = embed);
            }
            else
            {
                var message = await guildChannel
                    .GetMessageAsync(ulong.Parse(declarationMessageID));
                
                if (message == null)
                {
                    return false;
                }

                await guildChannel.DeleteMessageAsync(message);
                await SendDeclarationBotMessage();
            }

            return true;

        }

        /// <summary>
        /// 凸宣言した時のコード
        /// </summary>
        /// <returns></returns>
        private bool UserRegistorDeclareCommand()
        {
            var userReaction = m_userReaction;
            var userId = userReaction.UserId.ToString();

            var isExistSqlData = DeclareDataOnSQL(userId) .Any(d => d.FinishFlag == false);

            if (isExistSqlData)
            {
                return false;
            }

            var declarationData = UserToDeclareData(userId);
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

            // すでに宣言しているか判定
            var sqlData = DeclareDataOnSQL(userReaction.UserId.ToString())
                .Count(d => d.FinishFlag == false);

            if (sqlData != 1)
            {
                return false;
            }

            // 宣言終了
            var declarationData = UserToDeclareData(userReaction.UserId.ToString());
            declarationData.FinishFlag = true;
            var result = new MySQLDeclarationController()
                .UpdateDeclarationData(declarationData);

            // 予約の削除
            var playerData = new MySQLPlayerDataController().LoadPlayerData(
                userRole.Guild.Id.ToString(), userReaction.UserId.ToString());

            var mySQLReservationController = new MySQLReservationController();
            var reservationData = mySQLReservationController.LoadReservationData(playerData);
            var finishReservationData = reservationData
                .FirstOrDefault(d => d.BattleLap == userClanData.BattleLap && d.BossNumber == userClanData.BossNumber);

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

            var sqlDataSet = DeclareDataOnSQL(userReaction.UserId.ToString());

            var sqlData = sqlDataSet.FirstOrDefault(d => d.FinishFlag == false);

            if (sqlData != null)
            {
                new MySQLDeclarationController().DeleteDeclarationData(sqlData);
            }

            return true;
        }

        /// <summary>
        /// 凸宣言削除
        /// </summary>
        /// <returns></returns>
        private bool DeleteAllBattleData()
        {
            var userClanData = m_userClanData;

            var mySQLDeclaration = new MySQLDeclarationController();
            var declarationData = mySQLDeclaration.LoadDeclarationData(userClanData);

            var result = mySQLDeclaration.DeleteDeclarationData(declarationData);

            return result;
        }

        /// <summary>
        /// 次のボスに行く際のコード
        /// </summary>
        /// <returns></returns>
        private async Task<bool> NextBossCommand()
        {
            UserFinishBattleCommand();

            DeleteAllBattleData();

            if (m_userClanData.BossNumber == MaxBossNumber)
            {
                m_userClanData.BossNumber = MinBossNumber;
                m_userClanData.BattleLap += 1;
            }
            else
            {
                m_userClanData.BossNumber += 1;
            }
            new MySQLClanDataController().UpdateClanData(m_userClanData);

            await SendDeclarationBotMessage();

            return true;
        }

        /// <summary>
        /// userID情報からDeclarationDataを作成します。
        /// ボス情報はclanDataから読み出して使用します。
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
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
                FinishFlag = false
            };
        }

        private IEnumerable<DeclarationData> DeclareDataOnSQL(string userID)
        {
            var userRole = m_userRole;

            var playerData = new MySQLPlayerDataController().LoadPlayerData(
                userRole.Guild.Id.ToString(), userID);

            var declarationData = new MySQLDeclarationController()
                .LoadDeclarationData(playerData);

            return declarationData;
        }

        private async Task AttacheDefaultReaction(IUserMessage message)
        {

            string[] emojiData = { "⚔️", "✅", "🏁", "❌" };
            var emojiMatrix = emojiData.Select(x => new Emoji(x)).ToArray();

            //foreach (var emoji in emojiMatrix)
            {
                await message.AddReactionsAsync(emojiMatrix);
            }
        }

        private async Task RemoveUserReaction()
        {
            var textChannnel = m_userRole.Guild.GetTextChannel(
                ulong.Parse(m_userClanData.ChannelIDs.DeclarationChannelID));

            var message = await textChannnel.GetMessageAsync(m_userReaction.MessageId);

            if (message == null)
            {
                return;
            }
            await message.RemoveReactionAsync(m_userReaction.Emote, m_userReaction.User.Value);
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
                .OrderBy(d => BitConverter.ToUInt64(d.DateTime))
                .Select(d => d.PlayerData.GuildUserName)
                .ToList();

            var reservationListMessage = NameListToMessageData(reservationNameList);
            var reservationPlayerCount = reservationNameList.Count();
            if (reservationPlayerCount == 0)
            {
                reservationListMessage = "予約なし";
            }

            var finishNameList = declarationDataList
                .Where(d => d.FinishFlag)
                .OrderBy(d => BitConverter.ToUInt64(d.DateTime))
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
                .OrderBy(d => BitConverter.ToUInt64(d.DateTime))
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

            var explainMessage = "```python\n" +
                "1. ⚔️で本戦開始の宣言をします。\n" +
                "2. (ボスを倒さず)本戦が終わったら✅で完了します。\n" +
                "3. ボスを倒したら🏁を押してください。\n" +
                "4. 凸宣言をキャンセルするときは❌\n" +
                "```\n";

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

            embedBuild.AddField(new EmbedFieldBuilder()
            {
                IsInline = false,
                Name = $"説明",
                Value = explainMessage
            });

            embedBuild.Color = Color.Red;
            embedBuild.Footer = new EmbedFooterBuilder()
            {
                Text = $"最終更新時刻:{updateTimeString}"
            };

            var embed = embedBuild.Build();

            return embed;
        }

        private string CreateDeclarationDataMessage(ClanData clanData)
        {
            var reservationDataList =
                new MySQLReservationController().LoadBossLapReservationData(clanData);

            var reservationIDList = reservationDataList
               .OrderBy(d => BitConverter.ToUInt64(d.DateTime))
               .Select(d => d.PlayerData.UserID)
               .ToList();

            var messageData = "";
            foreach ( var reservationID in reservationIDList)
            {
                messageData += MentionUtils.MentionUser(ulong.Parse(reservationID));
                messageData += " ";
            }

            return messageData;

        }

        private string NameListToMessageData(List<string> nameDataSet)
        {
            var messageData = string.Empty;
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
