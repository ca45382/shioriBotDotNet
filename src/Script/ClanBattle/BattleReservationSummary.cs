using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ShioriBot.Net.Database;
using ShioriBot.Net.Model;
using ShioriBot.Net.DataType;
using ShioriBot.Net.Define;

namespace ShioriBot.Net.Script
{
    public class BattleReservationSummary
    {
        private readonly ClanData m_ClanData;
        private readonly SocketRole m_Role;
        private readonly SocketTextChannel m_SocketTextChannel;

        public BattleReservationSummary(SocketRole role, ClanData clanData = null)
        {
            m_Role = role;

            if (clanData == null)
            {
                m_ClanData = DatabaseClanDataController.LoadClanData(m_Role);
            }
            else
            {
                m_ClanData = clanData;
            }

            m_SocketTextChannel = m_Role.Guild.GetChannel(m_ClanData.GetChannelID(ChannelFeatureType.ReserveResultID)) as SocketTextChannel;
        }

        public async Task RunInteraction(SocketMessageComponent messageComponent)
        {
            if (!Enum.TryParse<ButtonType>(messageComponent.Data.CustomId, out var buttonType))
            {
                return;
            }

            if (buttonType == ButtonType.Reload)
            {
                await UpdateMessage();
            }
        }

        /// <summary>
        /// 凸予約一覧チャンネルにメッセージを送信する。
        /// </summary>
        /// <returns></returns>
        public async Task SendMessage()
        {
            var embedData = CreateEmbed();
            var componentData = CreateComponent();
            var sendedMessageData = await m_SocketTextChannel.SendMessageAsync(embed: embedData, component: componentData);
            DatabaseMessageDataController.UpdateMessageID(m_ClanData, sendedMessageData.Id, MessageFeatureType.ReserveResultID);
        }

        public async Task UpdateMessage()
        {
            if (m_SocketTextChannel == null)
            {
                return;
            }

            var reservationMessageID = m_ClanData.GetMessageID(MessageFeatureType.ReserveResultID);
            var cachedMessage = m_SocketTextChannel.GetCachedMessage(reservationMessageID);
            var embedData = CreateEmbed();
            var componentData = CreateComponent();

            if (cachedMessage is SocketUserMessage serverMessage)
            {
                await serverMessage.ModifyAsync(x => x.Embed = embedData);
            }
            else
            {
                var message = await m_SocketTextChannel.GetMessageAsync(reservationMessageID);
                await SendMessage();

                if (message != null)
                {
                    await m_SocketTextChannel.DeleteMessageAsync(message);
                }
            }
        }

        public void DeleteUnusedData(byte bossNumber)
        {
            var clanReservationData = DatabaseReservationController.LoadReservationData(m_ClanData, bossNumber);
            var bossLap = m_ClanData.GetBossLap(bossNumber);
            var deleteData = clanReservationData.Where(x => x.BattleLap < bossLap);
            DatabaseReservationController.DeleteReservationData(deleteData);
        }

        /// <summary>
        /// 予約メッセージを作成する
        /// </summary>
        /// <param name="clanData"></param>
        /// <returns></returns>
        private Embed CreateEmbed()
        {
            var reservationDataSet = DatabaseReservationController.LoadReservationData(m_ClanData);
            List<List<ReservationData>> reservationDataList = new();

            // TODO : Linqで行けそうな気がする
            for (var i = 0; i < CommonDefine.MaxBossNumber; i++)
            {
                reservationDataList.Add(new List<ReservationData>());
            }

            reservationDataSet.ForEach(x => reservationDataList[x.BossNumber - 1].Add(x));
            EmbedBuilder embedBuilder = new();

            for (var i = 0; i < CommonDefine.MaxBossNumber; i++)
            {
                EmbedFieldBuilder fieldBuilder = new();

                if (!reservationDataList[i].Any())
                {
                    // 空行を保つためのゼロ幅空白(\u200b)を挿入している。
                    fieldBuilder.Value = "\n\u200b";
                }
                else
                {
                    StringBuilder messageData = new();
                    messageData.AppendLine("```python");
                    reservationDataList[i].ForEach(x => messageData.AppendLine($"{x.BattleLap,2}周目 {x.PlayerData.GuildUserName} {x.CommentData}"));
                    messageData.AppendLine("```");
                    fieldBuilder.Value = messageData.ToString();
                }

                fieldBuilder.Name = $"{i + 1}ボス({reservationDataList[i].Count}件)";
                embedBuilder.AddField(fieldBuilder);
            }

            embedBuilder.Title = $"現在の予約状況:計{reservationDataSet.Count}件";

            return embedBuilder.Build();
        }

        private MessageComponent CreateComponent()
        {
            ComponentBuilder componentBuilder = new();
            componentBuilder.WithButton(
                ButtonType.Reload.ToLongLabel(),
                ButtonType.Reload.ToString(),
                ButtonStyle.Secondary,
                ButtonType.Reload.ToEmoji()
            );

            return componentBuilder.Build();
        }
    }
}
