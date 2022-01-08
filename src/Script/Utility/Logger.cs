using System.Threading.Tasks;
using Discord.WebSocket;
using ShioriBot.Net.DataType;
using ShioriBot.Net.Model;

namespace ShioriBot.Net.Script
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
