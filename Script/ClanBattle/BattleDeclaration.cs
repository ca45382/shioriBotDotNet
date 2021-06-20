using System;
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
    class BattleDeclaration : BaseClass
    {
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
            m_userRole = (channel as SocketGuildChannel)?.Guild.GetRole(m_userClanData.ClanRoleID);
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
            var declarationMessageID = m_userClanData.MessageData
                .GetMessageID(m_userClanData.ClanID, MessageFeatureType.DeclareID);

            if (declarationMessageID == 0 || m_userReaction.MessageId != declarationMessageID)
            {
                return;
            }

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

                case "🔄":
                    await UpdateDeclarationBotMessage();
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
                || !(byte.TryParse(splitMessageContent[1], out byte battleLap) && battleLap > 0)
                || !(byte.TryParse(splitMessageContent[2], out byte bossNumber) && bossNumber <= Define.Common.MaxBossNumber && bossNumber >= Define.Common.MinBossNumber))
            {
                return false;
            }

            SetAllBossLaps(bossNumber, battleLap);

            if (!new DatabaseClanDataController().UpdateClanData(m_userClanData))
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
            var embed = CreateDeclarationDataEmbed(m_userClanData);
            var content = CreateDeclarationDataMessage(m_userClanData);
            var declarationChannelID = m_userClanData.ChannelData
                .GetChannelID(m_userClanData.ClanID, ChannelFeatureType.DeclareID);

            if (declarationChannelID == 0)
            {
                return false;
            }

            var declarationChannel = m_userRole.Guild.GetTextChannel(declarationChannelID);
            var sentMessage = await declarationChannel.SendMessageAsync(text: content, embed: embed);

            if (sentMessage == null)
            {
                return false;
            }

            var sentMessageId = sentMessage.Id;

            var result = new DatabaseMessageDataController()
                .UpdateMessageID(m_userClanData, sentMessageId, MessageFeatureType.DeclareID);

            await AttacheDefaultReaction(sentMessage);

            return result;
        }

        /// <summary>
        /// 宣言データをアップデートするためのコード
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateDeclarationBotMessage()
        {
            var userClanData = m_userClanData;
            var userRole = m_userRole;

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
            var userReaction = m_userReaction;
            var userId = userReaction.UserId;

            var isExistSqlData = DeclareDataOnSQL(userId) .Any(d => d.FinishFlag == false);

            if (isExistSqlData)
            {
                return false;
            }

            var declarationData = UserToDeclareData(userId);
            var result = new DatabaseDeclarationController()
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
            var sqlData = DeclareDataOnSQL(userReaction.UserId)
                .Count(d => d.FinishFlag == false);

            if (sqlData != 1)
            {
                return false;
            }

            // 宣言終了
            var declarationData = UserToDeclareData(userReaction.UserId);
            declarationData.FinishFlag = true;
            var result = new DatabaseDeclarationController()
                .UpdateDeclarationData(declarationData);

            // 予約の削除
            var playerData = new DatabasePlayerDataController().LoadPlayerData(
                userRole.Guild.Id, userReaction.UserId);

            var mySQLReservationController = new DatabaseReservationController();
            var reservationData = mySQLReservationController.LoadReservationData(playerData);
            var finishReservationData = reservationData
                .FirstOrDefault(d => d.BattleLap == userClanData.GetNowLap() && d.BossNumber == userClanData.GetNowLap());

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

            var sqlDataSet = DeclareDataOnSQL(userReaction.UserId);

            var sqlData = sqlDataSet.FirstOrDefault(d => d.FinishFlag == false);

            if (sqlData != null)
            {
                new DatabaseDeclarationController().DeleteDeclarationData(sqlData);
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

            var mySQLDeclaration = new DatabaseDeclarationController();
            var declarationData = mySQLDeclaration.LoadDeclarationData(userClanData, userClanData.GetNowBoss());

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

            var nowBossNumber = m_userClanData.GetNowBoss();
            var nowBattleLap = m_userClanData.GetNowLap();

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
            new DatabaseClanDataController().UpdateClanData(m_userClanData);
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
            var userClanData = m_userClanData;
            var userRole = m_userRole;

            var playerData = new DatabasePlayerDataController().LoadPlayerData(
                userRole.Guild.Id, userID);

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
            var userRole = m_userRole;

            var playerData = new DatabasePlayerDataController().LoadPlayerData(
                userRole.Guild.Id, userID);

            var declarationData = new DatabaseDeclarationController()
                .LoadDeclarationData(playerData, m_userClanData.GetNowBoss());

            return declarationData;
        }

        private async Task AttacheDefaultReaction(IUserMessage message)
        {

            string[] emojiData = { "⚔️", "✅", "🏁", "❌", "🔄" };
            var emojiMatrix = emojiData.Select(x => new Emoji(x)).ToArray();

            //foreach (var emoji in emojiMatrix)
            {
                await message.AddReactionsAsync(emojiMatrix);
            }
        }

        private async Task RemoveUserReaction()
        {
            var declarationChannelID = m_userClanData.ChannelData
                .GetChannelID(m_userClanData.ClanID, ChannelFeatureType.DeclareID);
            var textChannnel = m_userRole.Guild.GetTextChannel(declarationChannelID);

            if(textChannnel == null)
            {
                return;
            }

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
                new DatabaseReservationController().LoadBossLapReservationData(clanData, clanData.GetNowBoss());
            var declarationDataList =
                new DatabaseDeclarationController().LoadDeclarationData(clanData, clanData.GetNowBoss());

            var bossNumber = clanData.GetNowBoss();
            var battleLap = clanData.GetNowLap();

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
                new DatabaseReservationController().LoadBossLapReservationData(clanData, clanData.GetNowBoss());

            var reservationIDList = reservationDataList
               .OrderBy(d => d.CreateDateTime)
               .Select(d => d.PlayerData.UserID)
               .ToList();

            var messageData = "";
            foreach ( var reservationID in reservationIDList)
            {
                messageData += MentionUtils.MentionUser(reservationID);
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

        /// <summary>
        /// 今のボス・周回数から ボスごとの周回数に変更して代入。
        /// 7月削除予定。
        /// </summary>
        /// <param name="bossNumber"></param>
        /// <param name="battleLap"></param>
        [Obsolete]
        private void SetAllBossLaps(int bossNumber, int battleLap)
        {
            if (bossNumber <= 0 || bossNumber > 5 || battleLap < 0 || m_userClanData == null)
            {
                return;
            }

            for (int i = 0; i < Define.Common.MaxBossNumber; i++)
            {
                if (i >= bossNumber)
                {
                    m_userClanData.SetBossLap(i + 1, battleLap - 1);
                }
                else
                {
                    m_userClanData.SetBossLap(i + 1, battleLap);
                }
            }
        }
    }
}
