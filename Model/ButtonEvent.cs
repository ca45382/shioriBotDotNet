using System.Linq;
using Discord.WebSocket;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Interface;

namespace PriconneBotConsoleApp.DataModel
{
    public class ButtonEvent : IPlayerEvent
    {
        public ButtonEvent(SocketInteraction socketInteraction)
        {
            SocketMessageComponent = (SocketMessageComponent)socketInteraction;
            User = socketInteraction.User as SocketGuildUser;
            Channel = socketInteraction.Channel as SocketTextChannel;
            PlayerData = DatabasePlayerDataController.LoadPlayerData(Channel.Guild.Id, User.Id);
            Role = Channel.Guild.GetRole(PlayerData?.ClanData.ClanRoleID ?? 0);

            if (Role == null)
            {
                return;
            }

            ClanData = DatabaseClanDataController.LoadClanData(Role);
            ChannelFeatureType = (ChannelFeatureType?)ClanData.ChannelData.FirstOrDefault(x => x.ChannelID == Channel.Id)?.FeatureID 
                ?? ChannelFeatureType.All;

        }

        public SocketMessageComponent SocketMessageComponent { get; }
        public ButtonType ButtonType { get; }
        public SocketGuildUser User { get; }
        public SocketTextChannel Channel { get; }
        public SocketRole Role { get; }
        public ClanData ClanData { get; }
        public PlayerData PlayerData { get; }
        public ChannelFeatureType ChannelFeatureType { get; }
    }
}
