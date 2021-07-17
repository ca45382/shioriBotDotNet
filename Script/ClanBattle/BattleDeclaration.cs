﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public class BattleDeclaration
    {
        private readonly ClanData m_UserClanData;
        private readonly SocketRole m_UserRole;
        private readonly SocketUserMessage m_UserMessage;
        private readonly SocketReaction m_UserReaction;
        private readonly int m_BossNumber;
        private readonly bool m_AllBattle = true;

        private BattleDeclaration(
            ClanData userClanData,
            ISocketMessageChannel channel,
            BossNumberType bossNumber = 0,
            SocketUserMessage userMessage = null,
            SocketReaction userReaction = null)
        {
            m_UserClanData = userClanData;
            m_UserRole = (channel as SocketGuildChannel)?.Guild.GetRole(m_UserClanData.ClanRoleID);
            m_UserMessage = userMessage;
            m_UserReaction = userReaction;

            if (bossNumber == 0)
            {
                m_AllBattle = false;
                m_BossNumber = m_UserClanData.GetNowBoss();
            }
            else
            {
                m_BossNumber = (int)bossNumber;
            }
            
        }

        public BattleDeclaration(ClanData userClanData, SocketUserMessage message, BossNumberType bossNumber = 0)
            : this(userClanData, message.Channel, bossNumber, userMessage: message)
        {
        }

        public BattleDeclaration(ClanData userClanData, SocketReaction reaction, BossNumberType bossNumber = 0)
            : this(userClanData, reaction.Channel, bossNumber,  userReaction: reaction)
        {
        }

        public async Task RunDeclarationCommandByMessage()
        {
            if (m_UserMessage != null && m_UserMessage.Content.StartsWith("!call"))
            {
                await DeclarationCallCommand();
                await new BattleReservation(m_UserClanData, m_UserMessage).UpdateSystemMessage();
            }
        }

        public async Task RunDeclarationCommandByReaction()
        {
            var declarationMessageID = m_UserClanData.MessageData
                .GetMessageID(m_UserClanData.ClanID, MessageFeatureType.DeclareID);

            if (declarationMessageID == 0 || m_UserReaction.MessageId != declarationMessageID)
            {
                return;
            }

            switch (m_UserReaction.Emote.Name)
            {
                case "⚔️":
                    UserRegistorDeclareCommand();
                    break;

                case "✅":
                    UserFinishBattleCommand();
                    break;

                case "🏁":
                    await NextBossCommand();
                    await new BattleReservation(m_UserClanData, m_UserReaction).UpdateSystemMessage();
                    return;

                case "❌":
                    UserDeleteBattleData();
                    break;

                case "🔄":
                    await UpdateDeclarationBotMessage();
                    break;
            }

            await UpdateDeclarationBotMessage();
            await RemoveUserReaction();
            await new BattleReservation(m_UserClanData, m_UserReaction).UpdateSystemMessage();
        }

        private async Task<bool> DeclarationCallCommand()
        {
            var splitMessageContent = m_UserMessage.Content.ZenToHan()
                .Split(new[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if (splitMessageContent.Length != 3
                || !(byte.TryParse(splitMessageContent[1], out byte battleLap) && battleLap > 0)
                || !(byte.TryParse(splitMessageContent[2], out byte bossNumber) && bossNumber <= Define.Common.MaxBossNumber && bossNumber >= Define.Common.MinBossNumber))
            {
                return false;
            }

            SetAllBossLaps(bossNumber, battleLap);

            if (!DatabaseClanDataController.UpdateClanData(m_UserClanData))
            {
                return false;
            }

            return await SendDeclarationBotMessage();
        }

        /// <summary>
        /// 宣言データを送信するためのコード
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SendDeclarationBotMessage()
        {
            var embed = CreateDeclarationDataEmbed(m_UserClanData);
            var content = CreateDeclarationDataMessage(m_UserClanData);
            var messageComponent = CreateDeclareComponent();
            var declarationChannelID = m_UserClanData.ChannelData
                .GetChannelID(m_UserClanData.ClanID, ChannelFeatureType.DeclareID);

            if (declarationChannelID == 0)
            {
                return false;
            }

            var declarationChannel = m_UserRole.Guild.GetTextChannel(declarationChannelID);
            var sentMessage = await declarationChannel.SendMessageAsync(text: content, embed: embed);

            if (sentMessage == null)
            {
                return false;
            }

            var sentMessageId = sentMessage.Id;

            var result = DatabaseMessageDataController
                .UpdateMessageID(m_UserClanData, sentMessageId, MessageFeatureType.DeclareID);

            await AttacheDefaultReaction(sentMessage);

            return result;
        }

        /// <summary>
        /// 宣言データをアップデートするためのコード
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateDeclarationBotMessage()
        {
            var userClanData = m_UserClanData;
            var userRole = m_UserRole;

            var declarationMessageID = userClanData.MessageData
                .GetMessageID(userClanData.ClanID, MessageFeatureType.DeclareID);
            var declarationChannelID = userClanData.ChannelData
                .GetChannelID(userClanData.ClanID, ChannelFeatureType.DeclareID);

            if (declarationMessageID == 0 || declarationChannelID == 0)
            {
                return false;
            }
            
            // ここでguildChannelは存在するチャンネルからしかメッセージが来ない
            var guildChannel = userRole.Guild.GetChannel(declarationChannelID) as SocketTextChannel;

            if (guildChannel.GetCachedMessage(declarationMessageID) is SocketUserMessage declarationBotMessage)
            {
                var embed = CreateDeclarationDataEmbed(userClanData);
                await declarationBotMessage.ModifyAsync(msg => msg.Embed = embed);
            }
            else
            {
                var message = await guildChannel
                    .GetMessageAsync(declarationMessageID);
                
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
            var userReaction = m_UserReaction;
            var userId = userReaction.UserId;

            var isExistSqlData = DeclareDataOnSQL(userId) .Any(d => d.FinishFlag == false);

            if (isExistSqlData)
            {
                return false;
            }

            var declarationData = UserToDeclareData(userId);
            var result = DatabaseDeclarationController.CreateDeclarationData(declarationData);

            return result;
        }

        /// <summary>
        /// 本戦が終了した際のコード
        /// </summary>
        /// <returns></returns>
        private bool UserFinishBattleCommand()
        {
            var userRole = m_UserRole;
            var userClanData = m_UserClanData;
            var userReaction = m_UserReaction;

            // すでに宣言しているか判定
            var sqlData = DeclareDataOnSQL(userReaction.UserId)
                .Count(d => d.FinishFlag == false);

            if (sqlData != 1)
            {
                return false;
            }

            // 宣言終了
            var declarationData = UserToDeclareData(userReaction.UserId);
            declarationData.FinishFlag = true;
            var result = DatabaseDeclarationController.UpdateDeclarationData(declarationData);

            // 予約の削除
            var playerData = DatabasePlayerDataController.LoadPlayerData(userRole, userReaction.UserId);
            var reservationData = DatabaseReservationController.LoadReservationData(playerData);
            var finishReservationData = reservationData
                .FirstOrDefault(d => d.BattleLap == userClanData.GetNowLap() && d.BossNumber == userClanData.GetNowBoss());

            if (finishReservationData != null)
            {
                DatabaseReservationController.DeleteReservationData(finishReservationData);
            }

            return result;

        }

        /// <summary>
        /// ユーザーがバトルをキャンセルした際のコード
        /// </summary>
        /// <returns></returns>
        private bool UserDeleteBattleData()
        {
            var userReaction = m_UserReaction;

            var sqlDataSet = DeclareDataOnSQL(userReaction.UserId);

            var sqlData = sqlDataSet.FirstOrDefault(d => d.FinishFlag == false);

            if (sqlData != null)
            {
                DatabaseDeclarationController.DeleteDeclarationData(sqlData);
            }

            return true;
        }

        /// <summary>
        /// 凸宣言削除
        /// </summary>
        /// <returns></returns>
        private bool DeleteAllBattleData()
        {
            var userClanData = m_UserClanData;
            var declarationData = DatabaseDeclarationController.LoadDeclarationData(userClanData, userClanData.GetNowBoss());
            var result = DatabaseDeclarationController.DeleteDeclarationData(declarationData);

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

            var nowBossNumber = m_UserClanData.GetNowBoss();
            var nowBattleLap = m_UserClanData.GetNowLap();

            if (nowBossNumber == Define.Common.MaxBossNumber)
            {
                nowBossNumber = Define.Common.MinBossNumber;
                nowBattleLap += 1;
            }
            else
            {
                nowBossNumber += 1;
            }

            SetAllBossLaps(nowBossNumber, nowBattleLap);
            DatabaseClanDataController.UpdateClanData(m_UserClanData);
            await SendDeclarationBotMessage();

            return true;
        }

        /// <summary>
        /// userID情報からDeclarationDataを作成します。
        /// ボス情報はclanDataから読み出して使用します。
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        private DeclarationData UserToDeclareData(ulong userID)
        {
            var userClanData = m_UserClanData;
            var userRole = m_UserRole;
            var playerData = DatabasePlayerDataController.LoadPlayerData(userRole, userID);

            return new DeclarationData()
            {
                PlayerID = playerData.PlayerID,
                BattleLap = userClanData.GetNowLap(),
                BossNumber = userClanData.GetNowBoss(),
                FinishFlag = false
            };
        }

        private IEnumerable<DeclarationData> DeclareDataOnSQL(ulong userID)
        {
            var userRole = m_UserRole;
            var playerData = DatabasePlayerDataController.LoadPlayerData(userRole, userID);
            var declarationData = DatabaseDeclarationController.LoadDeclarationData(playerData, m_UserClanData.GetNowBoss());

            return declarationData;
        }

        private async Task AttacheDefaultReaction(IUserMessage message)
        {

            string[] emojiData = { "⚔️", "✅", "🏁", "❌", "🔄" };
            var emojiMatrix = emojiData.Select(x => new Emoji(x)).ToArray();
            await message.AddReactionsAsync(emojiMatrix);
        }

        private async Task RemoveUserReaction()
        {
            var declarationChannelID = m_UserClanData.ChannelData
                .GetChannelID(m_UserClanData.ClanID, ChannelFeatureType.DeclareID);
            var textChannnel = m_UserRole.Guild.GetTextChannel(declarationChannelID);

            if(textChannnel == null)
            {
                return;
            }

            var message = await textChannnel.GetMessageAsync(m_UserReaction.MessageId);

            if (message == null)
            {
                return;
            }

            await message.RemoveReactionAsync(m_UserReaction.Emote, m_UserReaction.User.Value);
        }

        private Embed CreateDeclarationDataEmbed(ClanData clanData)
        {
            if (!m_AllBattle)
            {
                return null;
            }

            var reservationDataList =
                DatabaseReservationController.LoadBossLapReservationData(clanData, m_BossNumber);
            var declarationDataList =
                DatabaseDeclarationController.LoadDeclarationData(clanData, (byte)m_BossNumber);

            var battleLap = clanData.GetBossLap(m_BossNumber);
            var updateTime = DateTime.Now;
            var updateTimeString = updateTime.ToString("T");

            var reservationNameList = reservationDataList
                .OrderBy(d => d.CreateDateTime)
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
                .Where(d => !d.FinishFlag)
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
                Title = $"凸宣言({battleLap, 2}周目{m_BossNumber,1}ボス)"
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
            if (!m_AllBattle)
            {
                return string.Empty;
            }

            var reservationDataList =
                DatabaseReservationController.LoadBossLapReservationData(clanData, m_BossNumber);

            var reservationIDList = reservationDataList
               .OrderBy(d => d.CreateDateTime)
               .Select(d => d.PlayerData.UserID)
               .ToList();

            var mentionList = reservationIDList.Select(x => MentionUtils.MentionUser(x));

            return string.Join(" ", mentionList);
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

        /// <summary>
        /// 今のボス・周回数から ボスごとの周回数に変更して代入。
        /// 7月削除予定。
        /// </summary>
        /// <param name="bossNumber"></param>
        /// <param name="battleLap"></param>
        [Obsolete]
        private void SetAllBossLaps(int bossNumber, int battleLap)
        {
            if (bossNumber <= 0 || bossNumber > 5 || battleLap < 0 || m_UserClanData == null)
            {
                return;
            }

            for (int i = 0; i < Define.Common.MaxBossNumber; i++)
            {
                if (i >= bossNumber)
                {
                    m_UserClanData.SetBossLap(i + 1, battleLap - 1);
                }
                else
                {
                    m_UserClanData.SetBossLap(i + 1, battleLap);
                }
            }
        }
    }
}
