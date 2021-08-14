using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Define;

namespace PriconneBotConsoleApp.Script
{
    public class BattleProgress
    {
        private readonly CommandEventArgs m_CommandEventArgs;
        private readonly BossNumberType m_BossNumberType;

        private class PlayerInfo
        {
            public PlayerData PlayerData;
            public ProgressData ProgressData;
            private uint BossRemainHP;

            public PlayerInfo(PlayerData playerData, ProgressData progressData, uint bossRemainHP = 0)
            {
                PlayerData = playerData;
                ProgressData = progressData;
                BossRemainHP = bossRemainHP;
            }

            public string GetNameWithData()
            {
                const string halfSizeWhitespace = " "; // TODO: Constantsで定義する
                const string fullSizeWhitespace = "　"; // TODO: Constantsで定義する

                return string.Join(
                    halfSizeWhitespace,
                    ((ProgressStatus) ProgressData.Status).ToLabel(),
                    ProgressData.CarryOverFlag ? "持" : fullSizeWhitespace,
                    $"{ProgressData.Damage,6}@{ProgressData.RemainTime:D2}",
                    ((AttackType)ProgressData.AttackType).ToShortLabel(),
                    DatabaseReportDataController.GetReportCount(PlayerData),
                    PlayerData.GuildUserName,
                    ProgressData.Status != 1 ? ProgressData.CommentData : string.Empty
                );
            }
        }

        public BattleProgress(CommandEventArgs commandEventArgs)
        {
            m_CommandEventArgs = commandEventArgs;
            m_BossNumberType = ChannelTypeToBossNumber(m_CommandEventArgs.ChannelFeatureType);
        }

        /// <summary>
        /// 進行を開始するコマンド。引数は「ボス番号」。
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            if (!ushort.TryParse(m_CommandEventArgs.Arguments[0], out var lap)
                || lap > ClanBattleDefine.MaxLapNumber || lap <= 0)
            {
                return;
            }

            await ChangeLap(lap);
        }

        public async Task SendList()
            => await SendClanProgressList();

        public async Task NextBoss()
            => await ChangeLap();

        /// <summary>
        /// 編成データをアップデートする。
        /// </summary>
        /// <param name="inputCommand"></param>
        /// <returns></returns>
        public async Task UpdateAttackData(AttackType attackType)
        {
            if (!TryGetProgressData(out var userProgressData))
            {
                userProgressData = new ProgressData
                {
                    PlayerID = m_CommandEventArgs.PlayerData.PlayerID,
                    BossNumber = (byte)m_BossNumberType,
                    BattleLap = (ushort)m_CommandEventArgs.ClanData.GetBossLap(m_BossNumberType),
                };
            }

            if (attackType == AttackType.CarryOver)
            {
                userProgressData.CarryOverFlag = true;
            }
            else
            {
                userProgressData.AttackType = (byte)attackType;
            }

            await UpdateProgressData(userProgressData, m_CommandEventArgs.PlayerData);
        }

        /// <summary>
        /// ダメージ・残り時間を抽出。
        /// </summary>
        /// <param name="inputCommand"></param>
        /// <returns></returns>
        public async Task UpdateDamageData()
        {
            var progressDataFlag = false;
            uint damageNumber = 0;
            byte remainTimeNumber = 0;

            if (Regex.IsMatch(m_CommandEventArgs.Name, @"\d+万$"))
            {
                if (!uint.TryParse(Regex.Match(m_CommandEventArgs.Name, @"\d+").ToString(), out damageNumber)
                   || damageNumber > CommonDefine.MaxDamageValue)
                {
                    return;
                }

                progressDataFlag = true;
            }

            if (Regex.IsMatch(m_CommandEventArgs.Name, @"\d+@\d+"))
            {
                var damageText = Regex.Match(m_CommandEventArgs.Name, @"\d+@").ToString();
                var remainTimeText = Regex.Match(m_CommandEventArgs.Name, @"@\d+").ToString();

                if (!uint.TryParse(Regex.Match(damageText, @"\d+").ToString(), out damageNumber)
                    || !byte.TryParse(Regex.Match(remainTimeText, @"\d+").ToString(), out remainTimeNumber)
                    || damageNumber > CommonDefine.MaxDamageValue || remainTimeNumber > CommonDefine.MaxBattleTime)
                {
                    return;
                }

                progressDataFlag = true;
            }

            if (!progressDataFlag)
            {
                return;
            }

            if (!TryGetProgressData(out var userProgressData))
            {
                userProgressData = new ProgressData()
                {
                    PlayerID = m_CommandEventArgs.PlayerData.PlayerID,
                    BossNumber = (byte)m_BossNumberType,
                    BattleLap = (ushort)m_CommandEventArgs.ClanData.GetBossLap(m_BossNumberType),
                };
            }

            if (damageNumber == 0)
            {
                userProgressData.Status = (byte)ProgressStatus.SOS;
            }

            userProgressData.Damage = damageNumber;
            userProgressData.RemainTime = remainTimeNumber;
            userProgressData.CommentData = string.Join(" ", m_CommandEventArgs.Arguments);
            await UpdateProgressData(userProgressData, m_CommandEventArgs.PlayerData);
        }

        /// <summary>
        /// 進行報告のステータス変更。引数 1か2。
        /// </summary>
        /// <param name="inputCommand"></param>
        /// <returns></returns>
        public async Task UpdateStatusData(ProgressStatus progressStatus)
        {
            if (!TryGetProgressData(out var userProgressData))
            {
                return;
            }

            userProgressData.Status = (byte)progressStatus;
            await UpdateProgressData(userProgressData, m_CommandEventArgs.PlayerData);
        }

        /// <summary>
        /// 進行データの削除や戻しを行います。
        /// </summary>
        /// <param name="deleteFlag"></param>
        /// <returns></returns>
        public async Task RemoveOrRevertUserData(bool deleteFlag = false)
        {
            var userData = m_CommandEventArgs.SocketUserMessage.MentionedUsers.FirstOrDefault();

            if (userData == null)
            {
                return;
            }

            var playerData = DatabasePlayerDataController.LoadPlayerData(m_CommandEventArgs.Role, userData.Id);

            var playerProgressData = DatabaseProgressController.GetProgressData(playerData, m_BossNumberType)
                .OrderByDescending(x => x.UpdateDateTime).FirstOrDefault();

            if (playerProgressData == null)
            {
                return;
            }

            if (deleteFlag)
            {
                DatabaseProgressController.DeleteProgressData(playerProgressData);
            }
            else
            {
                playerProgressData.Status = (byte)ProgressStatus.AttackReported;
                DatabaseProgressController.ModifyProgressData(playerProgressData);
            }

            await SendClanProgressList();
        }

        /// <summary>
        /// Initコマンドが入力されたとき
        /// </summary>
        /// <returns></returns>
        public async Task InitCommand()
        {
            InitializeProgressData();
            await SendClanProgressList();
        }

        private async Task UpdateProgressData(ProgressData progressData, PlayerData playerData)
        {

            if (progressData.Status == (byte)ProgressStatus.Unknown)
            {
                progressData.Status = (byte)ProgressStatus.AttackReported;
            }

            if (progressData.ProgressID == 0)
            {
                progressData.PlayerID = playerData.PlayerID;

                if (DatabaseProgressController.CreateProgressData(progressData))
                {
                    _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(new Emoji(ReactionType.Success.ToLabel()));
                    await SendClanProgressList();
                }
            }
            else
            {
                if (DatabaseProgressController.ModifyProgressData(progressData))
                {
                    _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(new Emoji(ReactionType.Success.ToLabel()));
                    await SendClanProgressList();
                }
            }
        }

        private async Task ChangeLap(ushort lap = 0)
        {
            InitializeProgressData();
            var bossLap = m_CommandEventArgs.ClanData.GetBossLap(m_BossNumberType);

            if (lap == 0)
            {
                m_CommandEventArgs.ClanData.SetBossLap(m_BossNumberType, bossLap + 1);
            }
            else
            {
                m_CommandEventArgs.ClanData.SetBossLap(m_BossNumberType, lap);
            }

            DatabaseClanDataController.UpdateClanData(m_CommandEventArgs.ClanData);
            await SendClanProgressList();
        }

        /// <summary>
        /// 凸進行報告状況の初期化
        /// </summary>
        /// <returns></returns>
        private void InitializeProgressData()
        {
            if(DatabaseProgressController.GetProgressData(m_CommandEventArgs.ClanData, m_BossNumberType) is { } deleteData)
            {
                DatabaseProgressController.DeleteProgressData(deleteData);
            }
        }

        private async Task SendClanProgressList(bool removeLastMessage = true)
        {
            var clanProgressEmbed = CreateProgressList();
            var sendMessage = await m_CommandEventArgs.Channel.SendMessageAsync(embed: clanProgressEmbed);

            if (sendMessage == null)
            {
                return;
            }

            var channelIDTemp = m_CommandEventArgs.ClanData.GetChannelID(BossNumberToChannelType(m_BossNumberType));
            var progressChannel = m_CommandEventArgs.Role.Guild.GetChannel(channelIDTemp) as SocketTextChannel;
            var lastMessageID = m_CommandEventArgs.ClanData.GetMessageID(BossNumberToMessageType(m_BossNumberType));
            DatabaseMessageDataController.UpdateMessageID(m_CommandEventArgs.ClanData, sendMessage.Id, BossNumberToMessageType(m_BossNumberType));
            var cachedMessage = progressChannel.GetCachedMessage(lastMessageID);

            if (cachedMessage == null)
            {
                try
                {
                    var message = await progressChannel.GetMessageAsync(lastMessageID);

                    if (removeLastMessage)
                    {
                        await message.DeleteAsync();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (removeLastMessage)
            {
                await cachedMessage.DeleteAsync();
            }
        }

        private Embed CreateProgressList()
        {
            var clanPlayerDataList = DatabasePlayerDataController.LoadPlayerData(m_CommandEventArgs.ClanData);
            var clanProgressData = DatabaseProgressController.GetProgressData(m_CommandEventArgs.ClanData, m_BossNumberType)
                .OrderBy(x => x.Status).ThenByDescending(x => x.Damage).ThenBy(x => x.CreateDateTime)
                .ToArray();

            var progressPlayer = clanProgressData.Select(x => 
                    new PlayerInfo(clanPlayerDataList.FirstOrDefault(y => y.PlayerID == x.PlayerID),x))
                .ToArray();

            var bossLap = m_CommandEventArgs.ClanData.GetBossLap(m_BossNumberType);
            var bossData = RediveClanBattleData.BossDataList
                .FirstOrDefault(x => x.BossNumber == (byte)m_BossNumberType && x.LapNumberFrom <= bossLap && (x.LapNumberTo == -1 || x.LapNumberTo >= bossLap));

            var summaryStringBuilder = new StringBuilder();
            // 持ち越しデータ出力
            summaryStringBuilder.AppendLine("凸済み:" + progressPlayer.Where(x => x.ProgressData.Status == (byte)ProgressStatus.AttackDone).Count() + "/"
                + "持ち越し中" + "0");

            var remainAttackString = new StringBuilder();
            remainAttackString.Append("残凸 ");
            var reportCount = DatabaseReportDataController.GetRemainPlayerCount(m_CommandEventArgs.ClanData);

            for (int i = CommonDefine.MaxReportNumber; i >= 0; i--)
            {
                remainAttackString.Append((i == 0 ? "完凸:" : i + "凸:") + reportCount[i] + "人 ");
            }

            summaryStringBuilder.AppendLine(remainAttackString.ToString());

            // ボスのHPをここに入力(万表示)
            var bossHP = bossData?.HP / CommonDefine.DisplayDamageUnit ?? 0;
            var sumAttackDoneHP = progressPlayer.Where(x => x.ProgressData.Status == (byte)ProgressStatus.AttackDone).Select(x => (int)x.ProgressData.Damage).Sum();
            var sumAttackReadyHP = progressPlayer.Where(x => x.ProgressData.Status == (byte)ProgressStatus.AttackReady).Select(x => (int)x.ProgressData.Damage).Sum();
            summaryStringBuilder.AppendLine("現在HP " + (bossHP - sumAttackDoneHP) + "万 / " + bossHP + "万");
            summaryStringBuilder.AppendLine("仮確HP " + (bossHP - sumAttackReadyHP - sumAttackDoneHP) + "万 / " + bossHP + "万");

            var headerFieldBuilder = new EmbedFieldBuilder()
            {
                Name = "概要",
                Value = summaryStringBuilder.ToString(),
            };

            var reportMessage = string.Join("\n", progressPlayer.Select(x => x.GetNameWithData()).ToArray());

            var embedFieldBuilder = new EmbedFieldBuilder()
            {
                Name = "参加者",
                Value = reportMessage.Length == 0 ? "参加者なし" : "```Python\n" + reportMessage + "\n```",
            };

            var embedBuilder = new EmbedBuilder();
            embedBuilder.AddField(headerFieldBuilder);
            embedBuilder.AddField(embedFieldBuilder);
            embedBuilder.Title = $"{m_CommandEventArgs.ClanData.GetBossLap(m_BossNumberType)}周目 {(uint)m_BossNumberType}ボス {bossData?.Name}";

            embedBuilder.Footer = new EmbedFooterBuilder()
            {
                Text = $"最終更新時刻 : {DateTime.Now:T}",
            };

            return embedBuilder.Build();
        }

        private ProgressData GetProgressData(PlayerData playerData = null)
        {
            if (playerData == null)
            {
                playerData = m_CommandEventArgs.PlayerData;
            }

            return DatabaseProgressController.GetProgressData(playerData, m_BossNumberType)
               .Where(x => x.Status != (byte)ProgressStatus.AttackDone || x.Status != (byte)ProgressStatus.SubdueBoss)
               .FirstOrDefault();
        }

        private bool TryGetProgressData(out ProgressData progressData)
            => (progressData = GetProgressData()) != null;

        private MessageFeatureType BossNumberToMessageType(BossNumberType bossNumberType)
        {
            return bossNumberType switch
            {
                BossNumberType.Boss1Number => MessageFeatureType.ProgressBoss1ID,
                BossNumberType.Boss2Number => MessageFeatureType.ProgressBoss2ID,
                BossNumberType.Boss3Number => MessageFeatureType.ProgressBoss3ID,
                BossNumberType.Boss4Number => MessageFeatureType.ProgressBoss4ID,
                BossNumberType.Boss5Number => MessageFeatureType.ProgressBoss5ID,
                _ => MessageFeatureType.ProgressID,
            };
        }

        private ChannelFeatureType BossNumberToChannelType(BossNumberType bossNumberType)
        {
            return bossNumberType switch
            {
                BossNumberType.Boss1Number => ChannelFeatureType.ProgressBoss1ID,
                BossNumberType.Boss2Number => ChannelFeatureType.ProgressBoss2ID,
                BossNumberType.Boss3Number => ChannelFeatureType.ProgressBoss3ID,
                BossNumberType.Boss4Number => ChannelFeatureType.ProgressBoss4ID,
                BossNumberType.Boss5Number => ChannelFeatureType.ProgressBoss5ID,
                _ => ChannelFeatureType.ProgressID,
            };
        }

        private BossNumberType ChannelTypeToBossNumber(ChannelFeatureType channelFeatureType)
        {
            return channelFeatureType switch
            {
                ChannelFeatureType.ProgressBoss1ID => BossNumberType.Boss1Number,
                ChannelFeatureType.ProgressBoss2ID => BossNumberType.Boss2Number,
                ChannelFeatureType.ProgressBoss3ID => BossNumberType.Boss3Number,
                ChannelFeatureType.ProgressBoss4ID => BossNumberType.Boss4Number,
                ChannelFeatureType.ProgressBoss5ID => BossNumberType.Boss5Number,
                _ => BossNumberType.Unknown,
            };
        }
    }
}
