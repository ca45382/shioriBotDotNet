using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.DataModel
{
    public class CommandEventArgs : EventArgs
    {
        public CommandEventArgs(SocketUserMessage socketUserMessage)
        {
            SocketUserMessage = socketUserMessage;

            if (SocketUserMessage.Content.Length == 0)
            {
                throw new ArgumentException("Content.Length が 0");
            }

            var splitContents = SocketUserMessage.Content.ZenToHan().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Name = splitContents[0];
            Arguments = splitContents.Length > 1 ? splitContents.Skip(1).ToList() : Array.Empty<string>();
            User = SocketUserMessage.Author as SocketGuildUser;
            Channel = SocketUserMessage.Channel as SocketTextChannel;
            PlayerData = DatabasePlayerDataController.LoadPlayerData(Channel.Guild.Id, SocketUserMessage.Author.Id);
            Role = Channel.Guild.GetRole(PlayerData?.ClanData.ClanRoleID ?? 0);

            if (Role == null)
            {
                return;
            }

            ClanData = DatabaseClanDataController.LoadClanData(Role);

            ChannelFeatureType =
                (ChannelFeatureType?)ClanData.ChannelData.FirstOrDefault(x => x.ChannelID == Channel.Id)?.FeatureID
                ?? ChannelFeatureType.All;
        }

        public SocketUserMessage SocketUserMessage { get; }
        public string Name { get; }
        public IReadOnlyList<string> Arguments { get; }
        public SocketGuildUser User { get; }
        public SocketTextChannel Channel { get; }
        public SocketRole Role { get; }
        public ClanData ClanData { get; }
        public PlayerData PlayerData { get; }
        public ChannelFeatureType ChannelFeatureType { get; }
    }
}
