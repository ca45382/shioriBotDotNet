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
            ChannelFeatureType = ChannelFeatureType.All;

            if (socketUserMessage.Content.Length == 0)
            {
                throw new ArgumentException("Content.Length が 0");
            }

            var splitContents = socketUserMessage.Content.ZenToHan().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Name = splitContents[0];

            if (splitContents.Length > 1)
            {
                Arguments = splitContents.Skip(1).ToList();
            }
            else
            {
                Arguments = Array.Empty<string>();
            }

            var messageChannel = socketUserMessage.Channel as SocketGuildChannel;
            Channel = messageChannel as SocketTextChannel;
            var guild = messageChannel.Guild;
            Author = socketUserMessage.Author as SocketGuildUser;
            var playerData = DatabasePlayerDataController.LoadPlayerData(guild.Id, Author.Id);
            Role = messageChannel.Guild.GetRole(playerData?.ClanData.ClanRoleID ?? 0);

            if (Role == null)
            {
                return;
            }

            ClanData = DatabaseClanDataController.LoadClanData(Role);
            var channelFeatureID = ClanData.ChannelData.FirstOrDefault(x => x.ChannelID == Channel.Id)?.FeatureID ?? 0;

            if (channelFeatureID != 0)
            {
                ChannelFeatureType = (ChannelFeatureType)channelFeatureID;
            }
        }

        public string Name { get; }
        public ChannelFeatureType ChannelFeatureType { get; }
        public IReadOnlyList<string> Arguments { get; }
        public SocketUserMessage SocketUserMessage { get; }
        public ClanData ClanData { get; }
        public SocketRole Role { get; }
        public SocketGuildUser Author { get; }
        public SocketTextChannel Channel { get; }
    }
}
