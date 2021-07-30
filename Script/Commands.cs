using System.Threading.Tasks;
using PriconneBotConsoleApp.Attribute;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public static class Commands
    {
        // 持ち越し関連
        [Command(AttackType.CarryOver,
            2,
            compatibleChannels: ChannelFeatureType.CarryOverID
        )]
        public static Task UpdateCarryOverData(CommandEventArgs commandEventArgs)
        {
            new BattleCarryOver(commandEventArgs).UpdateCarryOverData();
            return Task.CompletedTask;
        }

        [Command("消化",
            0,
            1,
            compatibleChannels: ChannelFeatureType.CarryOverID
        )]
        public static Task DeleteCarryOverData(CommandEventArgs commandEventArgs)
        {
            new BattleCarryOver(commandEventArgs).DeleteCarryOverData();
            return Task.CompletedTask;
        }

        [Command( "!rm" ,
            0,
            2,
            compatibleChannels: ChannelFeatureType.CarryOverID
        )]
        public static Task DeleteOtherPlayerCarryOverData(CommandEventArgs commandEventArgs)
        {
            if (commandEventArgs.Arguments.Count == 2)
            {
                new BattleCarryOver(commandEventArgs).DeleteOtherPlayerData();
            }
            else
            {
                new BattleCarryOver(commandEventArgs).DeleteCarryOverData();
            }
            
            return Task.CompletedTask;
        }

        [Command("!list",
            0,
            0,
            compatibleChannels: ChannelFeatureType.CarryOverID
        )]
        public static async Task DisplayCarryOverList(CommandEventArgs commandEventArgs)
        {
            await new BattleCarryOver(commandEventArgs).SendClanCarryOverList();
        }

        [Command("!init",
            0,
            0,
            compatibleChannels: ChannelFeatureType.CarryOverID
        )]
        public static Task InitAllCarryOverData(CommandEventArgs commandEventArgs)
        {
            new BattleCarryOver(commandEventArgs).InitAllData();
            return Task.CompletedTask;
        }

        [Command(
            compatibleChannels: new[]
            {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
            }
        )]
        public static async Task Progress(CommandEventArgs commandEventArgs)
            => await new BattleProgress(
                    commandEventArgs.ClanData,
                    commandEventArgs.SocketUserMessage,
                    (byte)commandEventArgs.ChannelFeatureType.GetBossNumberType())
                .RunByMessage();
    }
}
