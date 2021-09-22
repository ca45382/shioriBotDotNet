using System.Threading.Tasks;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Model;

namespace PriconneBotConsoleApp.Script
{
    public static class Logger
    {
        public static async Task SendLogMessage(SocketRole role, ClanData clanData, string logMessage)
        {
            var logChannel = role.Guild.GetTextChannel(clanData.GetChannelID(ChannelFeatureType.LogID));
            await logChannel.SendMessageAsync(logMessage);
        }
    }
}
