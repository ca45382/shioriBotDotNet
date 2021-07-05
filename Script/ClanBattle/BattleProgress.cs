using Discord;
using Discord.WebSocket;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Define;
using PriconneBotConsoleApp.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Script
{
    public class BattleProgress
    {
        private readonly SocketUserMessage m_UserMessage;
        private readonly SocketRole m_UserRole;
        private readonly ClanData m_UserClanData;
        private readonly SocketGuild m_Guild;
        private readonly byte m_BossNumber;

        private ProgressData m_UserProgressData;

        public BattleProgress(ClanData clanData, SocketUserMessage userMessage, byte bossNumber = 0)
        {
            if (clanData.RoleData == null || clanData.ChannelData == null || clanData.MessageData == null)
            {
                clanData = DatabaseClanDataController.LoadClanData(m_UserRole);
            }

            if (bossNumber == 0)
            {
                m_BossNumber = clanData.GetNowBoss();
            }
            else
            {
                m_BossNumber = bossNumber;
            }

            m_UserClanData = clanData;
            m_UserMessage = userMessage;
            m_Guild = (userMessage.Channel as SocketTextChannel)?.Guild;
            m_UserRole = m_Guild?.GetRole(clanData.ClanRoleID);
        }

        public async Task RunByMessage()
        {
            if (m_UserMessage.Content.StartsWith("!init"))
            {
                InitializeProgressData();
            }

            UpdateProgressData();

            return;
        }

        private bool UpdateProgressData()
        {
            var messageData = m_UserMessage.Content.ZenToHan().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var progressUser = m_UserMessage.Author;

            if (m_UserMessage.MentionedUsers.Count() == 1)
            {
                progressUser = m_UserMessage.MentionedUsers.FirstOrDefault();
            }

            var progressPlayerData = DatabasePlayerDataController.LoadPlayerData(m_UserRole, progressUser.Id);
            m_UserProgressData = DatabaseProgressController.GetProgressData(progressPlayerData)
               .Where(x => x.Status != (byte)ProgressStatus.AttackConfirm || x.Status != (byte)ProgressStatus.CarryOver)
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

            m_UserProgressData.BossNumber = m_BossNumber;
            m_UserProgressData.BattleLap = (ushort)m_UserClanData.GetBossLap(m_BossNumber);

            if (m_UserProgressData.ProgressID == 0)
            {
                m_UserProgressData.PlayerID = progressPlayerData.PlayerID;

                if (DatabaseProgressController.CreateProgressData(m_UserProgressData))
                {
                    Task.Run(() => m_UserMessage.AddReactionAsync(new Emoji(EnumMapper.I.GetString(ReactionType.Success))));
                    return true;
                }
            }
            else
            {
                if (DatabaseProgressController.ModifyProgressData(m_UserProgressData))
                {
                    Task.Run(() => m_UserMessage.AddReactionAsync(new Emoji(EnumMapper.I.GetString(ReactionType.Success))));
                    return true;
                }
            }

            return false;
        }

        private bool InitializeProgressData()
        {
            var deleteData = DatabaseProgressController.GetProgressData(m_UserClanData);

            if (deleteData == null)
            {
                return false;
            }

            if (DatabaseProgressController.DeleteProgressData(deleteData))
            {
                return true;
            }

            return false;
        }

        private bool UpdateAttackData(string inputCommand)
        {
            var attackNumber = (byte)ConversionAttackNumber.StringToAttackNumber(inputCommand);

            if (attackNumber != 0)
            {
                var attackType = attackNumber;

                if (m_UserProgressData == null)
                {
                    m_UserProgressData = new ProgressData();
                }

                if (attackType == 99)
                {
                    m_UserProgressData.CarryOverFlag = true;
                }
                else
                {
                    m_UserProgressData.AttackType = attackType;
                }

                return true;
            }

            return false;
        }

        private bool UpdateDamageData(string[] inputCommand)
        {
            var damageData = inputCommand.Select(x => Regex.Match(x, @"\d+万")).Where(x => x != null).FirstOrDefault().ToString();

            if (damageData != "")
            {
                if (!uint.TryParse(Regex.Match(damageData, @"\d+").ToString(), out uint damageNumber)
                   || damageNumber > Common.MaxDamageValue)
                {
                    return false;
                }

                if (m_UserProgressData == null)
                {
                    m_UserProgressData = new ProgressData();
                }

                if (damageNumber == 0)
                {
                    m_UserProgressData.Status = (byte)ProgressStatus.SaveOurSouls;
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
                    || damageNumber > Common.MaxDamageValue || remainTimeNumber > Common.MaxBattleTime)
                {
                    return false;
                }

                if (m_UserProgressData == null)
                {
                    m_UserProgressData = new ProgressData();
                }

                if (damageNumber == 0)
                {
                    m_UserProgressData.Status = (byte)ProgressStatus.SaveOurSouls;
                }

                m_UserProgressData.Damage = damageNumber;
                m_UserProgressData.RemainTime = remainTimeNumber;
                var dataIndex = Array.IndexOf(inputCommand, timeAndDamageData);
                m_UserProgressData.CommentData = string.Join(" ", inputCommand.Where((value, index) => index != dataIndex));

                return true;
            }

            return false;
        }

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
                "atk" or "凸確定" => m_UserProgressData.Status = (byte)ProgressStatus.AttackConfirm,
                "kari" or "仮確定" => m_UserProgressData.Status = (byte)ProgressStatus.TemporaryConfirm,
                "sos" or "ziko" or "jiko" or "事故" => m_UserProgressData.Status = (byte)ProgressStatus.SaveOurSouls,
                "〆確定" or "fin" => m_UserProgressData.Status = (byte)ProgressStatus.CarryOver,
                _ => 0,
            };

            if (dataUpdateFlag == 0 || SplitData.Length == 1 || !uint.TryParse(SplitData[1], out uint damegeData))
            {
                return false;
            }
            else if (m_UserProgressData.Status == (byte)ProgressStatus.CarryOver)
            {
                m_UserProgressData.RemainTime = (byte)damegeData;
                return true;
            }

            m_UserProgressData.Damage = damegeData;
            
            return true;
        }
    }
}
