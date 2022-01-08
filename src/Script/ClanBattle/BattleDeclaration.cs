using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ShioriBot.Net.Model;
using ShioriBot.Net.Database;
using ShioriBot.Net.DataType;
using ShioriBot.Net.Extension;
using ShioriBot.Net.Define;

namespace ShioriBot.Net.Script
{
    public class BattleDeclaration
    {
        private readonly ClanData m_UserClanData;
        private readonly SocketRole m_UserRole;
        private readonly IUser m_User;
        private readonly SocketTextChannel m_DeclarationChannel;
        private readonly SocketUserMessage m_UserMessage;
        private readonly SocketInteraction m_UserInteraction;
        private readonly ulong m_SystemMessageID;
        private readonly byte m_BossNumber;
        private readonly bool m_AllBattle = true;

        private BattleDeclaration(
            ClanData userClanData,
            ISocketMessageChannel channel,
            BossNumberType bossNumber)
        {
            m_UserClanData = userClanData;
            var channelData = m_UserClanData.ChannelData.FirstOrDefault(x => x.ChannelID == channel.Id);
            var guild = (channel as SocketGuildChannel)?.Guild;
            m_UserRole = guild?.GetRole(m_UserClanData.ClanRoleID);
            m_BossNumber = (byte)bossNumber;
            m_SystemMessageID = m_UserClanData.MessageData
                .FirstOrDefault(x => x.FeatureID == (uint)GetDeclareMessageType(m_BossNumber))?.MessageID ?? 0;

            if (channelData == null || channelData.FeatureID == (uint)GetDeclareChannelType(m_BossNumber))
            {
                m_DeclarationChannel = guild.GetTextChannel(channelData.ChannelID);
            }
        }

        public BattleDeclaration(ClanData userClanData, SocketUserMessage message, BossNumberType bossNumber)
            : this(userClanData, message.Channel, bossNumber)
        {
            m_UserMessage = message;
            m_User = message.Author;
        }

        public BattleDeclaration(ClanData userClanData, SocketInteraction interaction, BossNumberType bossNumber)
            : this(userClanData, interaction.Channel, bossNumber)
        {
            m_UserInteraction = interaction;
            m_User = interaction.User;
        }

        public BattleDeclaration(SocketRole role, BossNumberType bossType)
        {
            m_BossNumber = (byte)bossType;
            m_UserRole = role;
            m_UserClanData = DatabaseClanDataController.LoadClanData(role);
            var channelID = m_UserClanData.ChannelData.GetChannelID(m_UserClanData.ClanID, GetDeclareChannelType(m_BossNumber));

            if (channelID != 0)
            {
                m_DeclarationChannel = role.Guild.GetTextChannel(channelID);
            }
        }

        public async Task RunDeclarationCommandByMessage()
        {
            if (m_UserMessage == null || m_DeclarationChannel == null)
            {
                return;
            }

            if (m_UserMessage.Content.StartsWith("!call"))
            {
                await DeclarationCallCommand();
                var battleReservationSummary = new BattleReservationSummary(m_UserRole);
                battleReservationSummary.DeleteUnusedData(m_BossNumber);
                await battleReservationSummary.UpdateMessage();
            }
        }

        public async Task RunByInteraction()
        {
            if (m_DeclarationChannel == null)
            {
                return;
            }

            if (m_SystemMessageID == 0)
            {
                //TODO:そもそも宣言されていない場合ここにエラー文
                return;
            }

            var messageComponent = (SocketMessageComponent)m_UserInteraction;

            if (m_SystemMessageID != messageComponent.Message.Id
               || !Enum.TryParse<ButtonType>(messageComponent.Data.CustomId, out var buttonType))
            {
                return;
            }

            switch (buttonType)
            {
                case ButtonType.StartBattle:
                    UserRegistorDeclareCommand();
                    break;

                case ButtonType.FinishBattle:
                    UserFinishBattleCommand();
                    break;

                case ButtonType.SubdueBoss:
                    await NextBossCommand();
                    await new BattleReservationSummary(m_UserRole).UpdateMessage();
                    return;

                case ButtonType.CancelBattle:
                    UserDeleteBattleData();
                    break;
            }

            await UpdateDeclarationBotMessage();
            await new BattleReservationSummary(m_UserRole).UpdateMessage();
        }

        /// <summary>
        /// 宣言データをアップデートするためのコード
        /// </summary>
        /// <returns></returns>
        public async Task UpdateDeclarationBotMessage()
        {
            if (m_UserRole == null || m_BossNumber == 0 || m_DeclarationChannel == null)
            {
                return;
            }

            var declarationMessageID = m_UserClanData.MessageData
                .GetMessageID(m_UserClanData.ClanID, GetDeclareMessageType(m_BossNumber));

            if (declarationMessageID == 0)
            {
                return;
            }

            if (m_DeclarationChannel.GetCachedMessage(declarationMessageID) is SocketUserMessage declarationBotMessage)
            {
                var embed = CreateDeclarationDataEmbed();
                await declarationBotMessage.ModifyAsync(msg => msg.Embed = embed);
            }
            else
            {
                var message = await m_DeclarationChannel.GetMessageAsync(declarationMessageID);

                if (message == null)
                {
                    return;
                }

                await m_DeclarationChannel.DeleteMessageAsync(message);
                await SendDeclarationBotMessage();
            }
        }

        /// <summary>
        /// !callが実行された際の挙動
        /// </summary>
        /// <returns></returns>
        private async Task DeclarationCallCommand()
        {
            var splitMessageContent = m_UserMessage.Content.ZenToHan()
                .Split(new[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if (!m_AllBattle || splitMessageContent.Length != 2
                || !(int.TryParse(splitMessageContent[1], out int battleLap) && battleLap > 0))
            {
                return;
            }

            var declareBattleLap = Math.Clamp(battleLap, 0, ClanBattleDefine.MaxLapNumber);

            m_UserClanData.SetBossLap(m_BossNumber, declareBattleLap);

            if (!DatabaseClanDataController.UpdateClanData(m_UserClanData))
            {
                return;
            }

            await SendDeclarationBotMessage();
        }

        /// <summary>
        /// 宣言データを送信するためのコード
        /// </summary>
        /// <returns></returns>
        private async Task SendDeclarationBotMessage()
        {
            var embed = CreateDeclarationDataEmbed();
            var content = CreateDeclarationDataMessage();
            var component = CreateDeclareComponent();
            var sentMessage = await m_DeclarationChannel.SendMessageAsync(text: content, embed: embed, component: component);

            if (sentMessage == null)
            {
                return;
            }

            DatabaseMessageDataController.UpdateMessageID(m_UserClanData, sentMessage.Id, GetDeclareMessageType(m_BossNumber));
        }

        /// <summary>
        /// 凸宣言した時のコード
        /// </summary>
        /// <returns></returns>
        private void UserRegistorDeclareCommand()
        {
            var playerData = DatabasePlayerDataController.LoadPlayerData(m_UserRole, m_User.Id);

            if (DatabaseDeclarationController.LoadDeclarationData(playerData, m_BossNumber)
                .Any(x => !x.FinishFlag))
            {
                return;
            }

            var declarationData = UserToDeclareData(playerData);
            DatabaseDeclarationController.CreateDeclarationData(declarationData);
        }

        /// <summary>
        /// 本戦が終了した際のコード
        /// </summary>
        /// <returns></returns>
        private bool UserFinishBattleCommand()
        {
            var playerData = DatabasePlayerDataController.LoadPlayerData(m_UserRole, m_User.Id);
            var userDeclareData = DatabaseDeclarationController.LoadDeclarationData(playerData, m_BossNumber)
                .FirstOrDefault(x => !x.FinishFlag);

            if (userDeclareData == null)
            {
                return false;
            }

            userDeclareData.FinishFlag = true;
            var result = DatabaseDeclarationController.UpdateDeclarationData(userDeclareData);

            // 予約の削除
            var reservationData = DatabaseReservationController.LoadReservationData(playerData);
            var finishReservationData = reservationData
                .FirstOrDefault(d => d.BattleLap == m_UserClanData.GetBossLap(m_BossNumber) && d.BossNumber == m_BossNumber);

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
        private void UserDeleteBattleData()
        {
            var playerData = DatabasePlayerDataController.LoadPlayerData(m_UserRole, m_User.Id);
            var sqlData = DatabaseDeclarationController.LoadDeclarationData(playerData, m_BossNumber)
                .FirstOrDefault(x => !x.FinishFlag);

            if (sqlData == null)
            {
                return;
            }

            DatabaseDeclarationController.DeleteDeclarationData(sqlData);
        }

        /// <summary>
        /// 凸宣言削除
        /// </summary>
        /// <returns></returns>
        private void DeleteAllBattleData()
        {
            var declarationData = DatabaseDeclarationController.LoadDeclarationData(m_UserClanData, m_BossNumber);
            DatabaseDeclarationController.DeleteDeclarationData(declarationData);
        }

        /// <summary>
        /// 次のボスに行く際のコード
        /// </summary>
        /// <returns></returns>
        private async Task NextBossCommand()
        {
            UserFinishBattleCommand();
            DeleteAllBattleData();

            var nowBattleLap = m_UserClanData.GetBossLap(m_BossNumber);
            var nextBattleLap = Math.Clamp(nowBattleLap + 1, 0, ClanBattleDefine.MaxLapNumber);

            m_UserClanData.SetBossLap(m_BossNumber, nextBattleLap);
            DatabaseClanDataController.UpdateClanData(m_UserClanData);

            var battleReservationSummary = new BattleReservationSummary(m_UserRole);
            battleReservationSummary.DeleteUnusedData(m_BossNumber);
            await Task.WhenAll(SendDeclarationBotMessage(), battleReservationSummary.UpdateMessage());
        }

        /// <summary>
        /// userID情報からDeclarationDataを作成します。
        /// ボス情報はclanDataから読み出して使用します。
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        private DeclarationData UserToDeclareData(PlayerData playerData)
            => new DeclarationData()
            {
                PlayerID = playerData.PlayerID,
                BattleLap = (ushort)m_UserClanData.GetBossLap(m_BossNumber),
                BossNumber = m_BossNumber,
                FinishFlag = false
            };

        private MessageComponent CreateDeclareComponent()
        {
            ComponentBuilder componentBuilder = new();
            componentBuilder.WithButton(ButtonType.StartBattle.ToLongLabel(), ButtonType.StartBattle.ToString(), ButtonStyle.Secondary, ButtonType.StartBattle.ToEmoji());
            componentBuilder.WithButton(ButtonType.FinishBattle.ToLongLabel(), ButtonType.FinishBattle.ToString(), ButtonStyle.Secondary, ButtonType.FinishBattle.ToEmoji());
            componentBuilder.WithButton(ButtonType.SubdueBoss.ToLongLabel(), ButtonType.SubdueBoss.ToString(), ButtonStyle.Secondary, ButtonType.SubdueBoss.ToEmoji());
            componentBuilder.WithButton(ButtonType.CancelBattle.ToLongLabel(), ButtonType.CancelBattle.ToString(), ButtonStyle.Danger, ButtonType.CancelBattle.ToEmoji());

            return componentBuilder.Build();
        }

        private Embed CreateDeclarationDataEmbed()
        {
            if (!m_AllBattle)
            {
                return null;
            }

            var reservationDataList =
                DatabaseReservationController.LoadBossLapReservationData(m_UserClanData, m_BossNumber);
            var declarationDataList =
                DatabaseDeclarationController.LoadDeclarationData(m_UserClanData, m_BossNumber);
            var bossData = RediveClanBattleData.BossDataList
                .FirstOrDefault(x => x.BossNumber == m_BossNumber);

            var battleLap = m_UserClanData.GetBossLap(m_BossNumber);
            var updateTime = DateTime.Now;
            var updateTimeString = updateTime.ToString("T");

            var reservationNameList = reservationDataList
                .OrderBy(d => d.CreateDateTime)
                .Select(d => d.PlayerData.GuildUserName)
                .ToList();

            var reservationListMessage = NameListToMessageData(reservationNameList);
            if (reservationNameList.Count == 0)
            {
                reservationListMessage = "予約なし";
            }

            var finishNameList = declarationDataList
                .Where(d => d.FinishFlag)
                .OrderBy(d => d.DateTime)
                .Select(d => d.PlayerData.GuildUserName)
                .ToList();

            var finishListMessage = NameListToMessageData(finishNameList);
            if (finishNameList.Count == 0)
            {
                finishListMessage = "完了者なし";
            }

            var nowBattleNameList = declarationDataList
                .Where(d => !d.FinishFlag)
                .OrderBy(d => d.DateTime)
                .Select(d => d.PlayerData.GuildUserName)
                .ToList();

            var nowBattleListMessage = NameListToMessageData(nowBattleNameList);
            if (nowBattleNameList.Count == 0)
            {
                nowBattleListMessage = "宣言なし";
            }

            var embedBuild = new EmbedBuilder
            {
                Title = $"凸宣言({battleLap,2}周目{m_BossNumber,1}ボス {bossData?.Name})"
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
                Name = $"本戦宣言中({nowBattleNameList.Count}人)",
                Value = nowBattleListMessage
            });

            embedBuild.AddField(new EmbedFieldBuilder()
            {
                IsInline = true,
                Name = $"予約中({reservationNameList.Count}人)",
                Value = reservationListMessage
            });

            embedBuild.AddField(new EmbedFieldBuilder()
            {
                IsInline = false,
                Name = $"本戦完了({finishNameList.Count}人)",
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

        private string CreateDeclarationDataMessage()
        {
            if (!m_AllBattle)
            {
                return string.Empty;
            }

            var reservationDataList =
                DatabaseReservationController.LoadBossLapReservationData(m_UserClanData, m_BossNumber);

            var reservationIDList = reservationDataList
               .OrderBy(d => d.CreateDateTime)
               .Select(d => d.PlayerData.UserID);

            return string.Join(" ", reservationIDList.Select(x => MentionUtils.MentionUser(x)));
        }

        private string NameListToMessageData(IEnumerable<string> nameDataSet)
            => string.Join('\n', nameDataSet.Select((value, index) => $"{index + 1,2}. {value}"));

        private ChannelFeatureType GetDeclareChannelType(int bossNumber)
            => bossNumber switch
            {
                1 => ChannelFeatureType.DeclareBoss1ID,
                2 => ChannelFeatureType.DeclareBoss2ID,
                3 => ChannelFeatureType.DeclareBoss3ID,
                4 => ChannelFeatureType.DeclareBoss4ID,
                5 => ChannelFeatureType.DeclareBoss5ID,
                _ => 0,
            };

        private MessageFeatureType GetDeclareMessageType(int bossNumber)
            => bossNumber switch
            {
                1 => MessageFeatureType.DeclareBoss1ID,
                2 => MessageFeatureType.DeclareBoss2ID,
                3 => MessageFeatureType.DeclareBoss3ID,
                4 => MessageFeatureType.DeclareBoss4ID,
                5 => MessageFeatureType.DeclareBoss5ID,
                _ => 0,
            };
    }
}
