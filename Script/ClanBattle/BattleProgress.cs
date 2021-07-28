using System;
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
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public class BattleProgress
    {
        private readonly SocketUserMessage m_UserMessage;
        private readonly SocketRole m_UserRole;
        private readonly ClanData m_UserClanData;
        private readonly SocketGuild m_Guild;
        private readonly byte m_BossNumber;
        private readonly bool m_AllBattleFlag = true;

        private ProgressData m_UserProgressData;

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

        public BattleProgress(ClanData clanData, SocketUserMessage userMessage, byte bossNumber)
        {
            if (clanData.RoleData == null || clanData.ChannelData == null || clanData.MessageData == null)
            {
                clanData = DatabaseClanDataController.LoadClanData(m_UserRole);
            }
            
            m_BossNumber = bossNumber;
            m_UserClanData = clanData;
            m_UserMessage = userMessage;
            m_Guild = (userMessage.Channel as SocketTextChannel)?.Guild;
            m_UserRole = m_Guild?.GetRole(clanData.ClanRoleID);
        }

        public async Task RunByMessage()
        {
            if (m_UserMessage.Content.StartsWith("!"))
            {
                if (m_UserMessage.Content.StartsWith("!init"))
                {
                    InitializeProgressData();
                    return;
                }
                else if (m_UserMessage.Content.StartsWith("!list"))
                {
                    await SendClanProgressList();
                    return;
                }
                else if (m_UserMessage.Content.StartsWith("!next"))
                {
                    await ChangeLap();
                    return;
                }
                else if (m_UserMessage.Content.StartsWith("!call"))
                {
                    await CallProgress();
                    return;
                }
                else if (m_UserMessage.Content.StartsWith("!rm"))
                {
                    await RevertOrRevertUserData(true);
                    return;
                }
                else if (m_UserMessage.Content.StartsWith("!rv"))
                {
                    await RevertOrRevertUserData();
                    return;
                }
            }
            else
            {
                await UpdateProgressData();
            }
            
            return;
        }

        private async Task<bool> UpdateProgressData()
        {
            var messageData = m_UserMessage.Content.ZenToHan().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var progressUser = m_UserMessage.Author;

            if (m_UserMessage.MentionedUsers.Count() == 1)
            {
                progressUser = m_UserMessage.MentionedUsers.FirstOrDefault();
            }

            var progressPlayerData = DatabasePlayerDataController.LoadPlayerData(m_UserRole, progressUser.Id);
            m_UserProgressData = DatabaseProgressController.GetProgressData(progressPlayerData, (BossNumberType)m_BossNumber)
               .Where(x => x.Status != (byte)ProgressStatus.AttackDone || x.Status != (byte)ProgressStatus.CarryOver)
               .FirstOrDefault();

            var successFlag = false;

            if (UpdateAttackData(messageData[0]))
            {
                successFlag = true;
            }
            else if (UpdateDamageData(messageData))
            {
                successFlag = true;
            } 
            else if (m_UserProgressData == null)
            {
                return false;
            }
            else if (m_UserProgressData.ProgressID != 0 )
            {
                successFlag = UpdateStatusData(messageData);
            }

            if (!successFlag)
            {
                return false;
            }

            if (m_UserProgressData.Status == (byte)ProgressStatus.Unknown)
            {
                m_UserProgressData.Status = (byte)ProgressStatus.AttackReported;
            }

            m_UserProgressData.BossNumber = m_BossNumber;
            m_UserProgressData.BattleLap = (ushort)m_UserClanData.GetBossLap(m_BossNumber);

            if (m_UserProgressData.ProgressID == 0)
            {
                m_UserProgressData.PlayerID = progressPlayerData.PlayerID;

                if (DatabaseProgressController.CreateProgressData(m_UserProgressData))
                {
                    _ = m_UserMessage.AddReactionAsync(new Emoji(ReactionType.Success.ToLabel()));
                    await SendClanProgressList();
                    return true;
                }
            }
            else
            {
                if (DatabaseProgressController.ModifyProgressData(m_UserProgressData))
                {
                    _ = m_UserMessage.AddReactionAsync(new Emoji(ReactionType.Success.ToLabel()));
                    await SendClanProgressList();
                    return true;
                }
            }

            return false;
        }

        private async Task CallProgress()
        {
            var messageData = m_UserMessage.Content.ZenToHan().Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (messageData.Length != 2 || !ushort.TryParse(messageData[1], out var lap))
            {
                return;
            }

            await ChangeLap(lap);
        }

        private async Task<bool> ChangeLap(ushort lap = 0)
        {
            InitializeProgressData();
            var bossLap = m_UserClanData.GetBossLap(m_BossNumber);

            if (lap == 0)
            {
                m_UserClanData.SetBossLap(m_BossNumber, bossLap + 1);
            }
            else
            {
                m_UserClanData.SetBossLap(m_BossNumber, lap);
            }
            
            DatabaseClanDataController.UpdateClanData(m_UserClanData);
            await SendClanProgressList();

            return true;
        }

        private async Task RevertOrRevertUserData(bool deleteFlag = false)
        {
            var userData = m_UserMessage.MentionedUsers.FirstOrDefault();

            if (userData == null)
            {
                return;
            }

            var playerData = DatabasePlayerDataController.LoadPlayerData(m_UserRole, userData.Id);
            var playerProgressData = DatabaseProgressController.GetProgressData(playerData, (BossNumberType)m_BossNumber)
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
        /// 凸報告の初期化
        /// </summary>
        /// <returns></returns>
        private void InitializeProgressData()
        {
            var deleteData = DatabaseProgressController.GetProgressData(m_UserClanData, (BossNumberType)m_BossNumber);

            if (deleteData == null)
            {
                return;
            }

            if (DatabaseProgressController.DeleteProgressData(deleteData))
            {
                return;
            }

            return;
        }

        private async Task SendClanProgressList(bool removeLastMessage = true)
        {
            var clanProgressEmbed = CreateProgressData();
            var sendMessage = await m_UserMessage.Channel.SendMessageAsync(embed: clanProgressEmbed);
            var bossNumber = 0;

            if (sendMessage == null)
            {
                return;
            }

            if (m_AllBattleFlag)
            {
                bossNumber = m_BossNumber;
            }

            var progressChannel = m_Guild.GetChannel(m_UserClanData.ChannelData.GetChannelID(m_UserClanData.ClanID, BossNumberToChannelType(bossNumber))) as SocketTextChannel;
            var lastMessageID = m_UserClanData.MessageData.GetMessageID(m_UserClanData.ClanID, BossNumberToMessageType(bossNumber));
            DatabaseMessageDataController.UpdateMessageID(m_UserClanData, sendMessage.Id, BossNumberToMessageType(bossNumber));
            var lastMessage = progressChannel.GetCachedMessage(lastMessageID);

            if (lastMessage == null)
            {
                IMessage lastIMessage;

                try
                {
                    lastIMessage = await progressChannel.GetMessageAsync(lastMessageID);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }

                await lastIMessage.DeleteAsync();
                return;
            }

            await lastMessage.DeleteAsync();
            return;
        }

        /// <summary>
        /// 編成データをアップデートする。
        /// </summary>
        /// <param name="inputCommand"></param>
        /// <returns></returns>
        private bool UpdateAttackData(string inputCommand)
        {
            try
            {
                var attackType = EnumMapper.Parse<AttackType>(inputCommand);

                if (m_UserProgressData == null)
                {
                    m_UserProgressData = new ProgressData();
                }

                if (attackType == AttackType.CarryOver)
                {
                    m_UserProgressData.CarryOverFlag = true;
                }
                else
                {
                    m_UserProgressData.AttackType = (byte)attackType;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ダメージ・残り時間を抽出。
        /// </summary>
        /// <param name="inputCommand"></param>
        /// <returns></returns>
        private bool UpdateDamageData(string[] inputCommand)
        {
            var damageData = inputCommand.Select(x => Regex.Match(x, @"\d+万")).Where(x => x != null).FirstOrDefault().ToString();

            if (damageData != "")
            {
                if (!uint.TryParse(Regex.Match(damageData, @"\d+").ToString(), out uint damageNumber)
                   || damageNumber > CommonDefine.MaxDamageValue)
                {
                    return false;
                }

                if (m_UserProgressData == null)
                {
                    m_UserProgressData = new ProgressData();
                }

                if (damageNumber == 0)
                {
                    m_UserProgressData.Status = (byte)ProgressStatus.SOS;
                }

                m_UserProgressData.Damage = damageNumber;
                var dataIndex = Array.IndexOf(inputCommand, damageData);
                m_UserProgressData.CommentData = string.Join(" ", inputCommand.Where((value, index) => index != dataIndex));

                return true;
            }

            var timeAndDamageData = inputCommand.Select(x => Regex.Match(x, @"\d+@\d+")).Where(x => x != null).FirstOrDefault().ToString();

            if (timeAndDamageData != "")
            {
                var damageText = Regex.Match(timeAndDamageData, @"\d+@").ToString();
                var remainTimeText = Regex.Match(timeAndDamageData, @"@\d+").ToString();

                if (!uint.TryParse(Regex.Match(damageText, @"\d+").ToString(), out uint damageNumber)
                    || !byte.TryParse(Regex.Match(remainTimeText, @"\d+").ToString(), out byte remainTimeNumber)
                    || damageNumber > CommonDefine.MaxDamageValue || remainTimeNumber > CommonDefine.MaxBattleTime)
                {
                    return false;
                }

                if (m_UserProgressData == null)
                {
                    m_UserProgressData = new ProgressData();
                }

                if (damageNumber == 0)
                {
                    m_UserProgressData.Status = (byte)ProgressStatus.SOS;
                }

                m_UserProgressData.Damage = damageNumber;
                m_UserProgressData.RemainTime = remainTimeNumber;
                var dataIndex = Array.IndexOf(inputCommand, timeAndDamageData);
                m_UserProgressData.CommentData = string.Join(" ", inputCommand.Where((value, index) => index != dataIndex));

                return true;
            }

            return false;
        }

        /// <summary>
        /// 進行報告のステータス変更。
        /// </summary>
        /// <param name="inputCommand"></param>
        /// <returns></returns>
        private bool UpdateStatusData(string[] inputCommand)
        {
            if (m_UserProgressData == null)
            {
                return false;
            }

            int maxSplitNumber = 2;
            var SplitData = inputCommand[0].Split("@", maxSplitNumber, StringSplitOptions.RemoveEmptyEntries);

            var dataUpdateFlag = SplitData[0] switch
            {
                "atk" or "凸確定" => m_UserProgressData.Status = (byte)ProgressStatus.AttackDone,
                "kari" or "仮確定" => m_UserProgressData.Status = (byte)ProgressStatus.AttackReady,
                "sos" or "ziko" or "jiko" or "事故" => m_UserProgressData.Status = (byte)ProgressStatus.SOS,
                "〆確定" or "fin" => m_UserProgressData.Status = (byte)ProgressStatus.CarryOver,
                _ => 0,
            };

            if (dataUpdateFlag == 0)
            {
                return false;
            }

            if (SplitData.Length == 1 || !uint.TryParse(SplitData[1], out uint damegeData))
            {
                return true;
            }

            if (m_UserProgressData.Status == (byte)ProgressStatus.CarryOver)
            {
                m_UserProgressData.RemainTime = (byte)damegeData;
                return true;
            }

            m_UserProgressData.Damage = damegeData;
            
            return true;
        }

        private Embed CreateProgressData()
        {
            var clanProgressData = DatabaseProgressController.GetProgressData(m_UserClanData, (BossNumberType)m_BossNumber)
                .OrderBy(x => x.Status).ThenByDescending(x => x.Damage).ThenBy(x => x.CreateDateTime)
                .ToArray();
            var clanPlayerDataList = DatabasePlayerDataController.LoadPlayerData(m_UserClanData);
            var progressPlayer = clanProgressData.Select(x => new PlayerInfo(
                clanPlayerDataList.FirstOrDefault(y => y.PlayerID == x.PlayerID),
                x
                )).ToArray();
            var bossLap = m_UserClanData.GetBossLap(m_BossNumber);
            var bossData = RediveClanBattleData.BossDataList
                .FirstOrDefault(x => x.BossNumber == m_BossNumber && x.LapNumberFrom <= bossLap && (x.LapNumberTo == -1 || x.LapNumberTo >= bossLap));

            var summaryStringBuilder = new StringBuilder();
            // 持ち越しデータ出力
            summaryStringBuilder.AppendLine("凸済み:" + progressPlayer.Where(x => x.ProgressData.Status == (byte)ProgressStatus.AttackDone).Count() + "/"
                + "持ち越し中" + "0");

            var remainAttackString = new StringBuilder();
            remainAttackString.Append("残凸 ");
            var reportCount = DatabaseReportDataController.GetRemainPlayerCount(m_UserClanData);

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
            embedBuilder.Title = $"{m_UserClanData.GetBossLap(m_BossNumber)}周目 {m_BossNumber}ボス {bossData?.Name}";

            embedBuilder.Footer = new EmbedFooterBuilder()
            {
                Text = $"最終更新時刻 : {DateTime.Now:T}",
            };

            return embedBuilder.Build();
        }

        private MessageFeatureType BossNumberToMessageType(int bossNumber)
        {
            return bossNumber switch
            {
                (int)BossNumberType.Boss1Number => MessageFeatureType.ProgressBoss1ID,
                (int)BossNumberType.Boss2Number => MessageFeatureType.ProgressBoss2ID,
                (int)BossNumberType.Boss3Number => MessageFeatureType.ProgressBoss3ID,
                (int)BossNumberType.Boss4Number => MessageFeatureType.ProgressBoss4ID,
                (int)BossNumberType.Boss5Number => MessageFeatureType.ProgressBoss5ID,
                _ => MessageFeatureType.ProgressID,
            };
        }

        private ChannelFeatureType BossNumberToChannelType(int bossNumber)
        {
            return bossNumber switch
            {
                (int)BossNumberType.Boss1Number => ChannelFeatureType.ProgressBoss1ID,
                (int)BossNumberType.Boss2Number => ChannelFeatureType.ProgressBoss2ID,
                (int)BossNumberType.Boss3Number => ChannelFeatureType.ProgressBoss3ID,
                (int)BossNumberType.Boss4Number => ChannelFeatureType.ProgressBoss4ID,
                (int)BossNumberType.Boss5Number => ChannelFeatureType.ProgressBoss5ID,
                _ => ChannelFeatureType.ProgressID,
            };
        }
    }
}
