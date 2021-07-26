using System.Threading.Tasks;
using PriconneBotConsoleApp.Attribute;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public static class Commands
    {
        [Command(
            channelFeatureType: new[] { ChannelFeatureType.CarryOverID }
        )]
        public static async Task BattleCarryOver(CommandEventArgs commandEventArgs)
            => await new BattleCarryOver(commandEventArgs.ClanData, commandEventArgs.SocketUserMessage).RunByMessage();

        [Command(
            channelFeatureType: new[]{ 
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
            }
        )]
        public static async Task Progress(CommandEventArgs commandEventArgs)
            => await new BattleProgress(commandEventArgs.ClanData, commandEventArgs.SocketUserMessage, (byte)commandEventArgs.ChannelFeatureType.GetBossNumberType())
                .RunByMessage();
    }
}
