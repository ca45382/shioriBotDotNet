using Discord.WebSocket;
using PriconneBotConsoleApp.Model;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Interface
{
    public interface IPlayerEvent
    {
        public SocketGuildUser User { get; }
        public SocketTextChannel Channel { get; }
        public SocketRole Role { get; }
        public ClanData ClanData { get; }
        public PlayerData PlayerData { get; }
        public ChannelFeatureType ChannelFeatureType { get; }
    }
}
