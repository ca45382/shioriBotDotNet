using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PriconneBotConsoleApp.Attribute;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Script
{
    public static class CommandMapper
    {
        private readonly static ConcurrentDictionary<(ChannelFeatureType, string), (CommandAttribute Info, Func<CommandEventArgs, Task> Handler)> m_CommandCache = new();

        public static void InitCommandCache()
        {
            var searchOption = BindingFlags.Static | BindingFlags.Public;

            foreach (var methodInfo in typeof(Commands).GetMethods(searchOption).AsEnumerable())
            {
                if (methodInfo.GetCustomAttribute<CommandAttribute>(false) is not CommandAttribute commandAttribute)
                {
                    continue;
                }

                foreach (var commandName in commandAttribute.Names)
                {
                    foreach (var channelFeatureType in commandAttribute.ChannelTypes)
                    {
                        m_CommandCache[(channelFeatureType, commandName)] = (commandAttribute, (Func<CommandEventArgs, Task>)Delegate.CreateDelegate(typeof(Func<CommandEventArgs, Task>), methodInfo));
                    }
                }
            }

            return;
        }

        public static async Task Invoke(CommandEventArgs commandEventArgs)
        {
            if ((m_CommandCache.TryGetValue((commandEventArgs.ChannelFeatureType, commandEventArgs.Name), out var functionData)
                || m_CommandCache.TryGetValue((ChannelFeatureType.All, commandEventArgs.Name), out functionData)
                || m_CommandCache.TryGetValue((commandEventArgs.ChannelFeatureType, string.Empty), out functionData)))
            {
                if (functionData.Info.IsCompatibleArgumentLength(commandEventArgs.Arguments.Count))
                {
                    await functionData.Handler(commandEventArgs);
                }
                else
                {
                    throw new ArgumentException(
                        $"Argument length {commandEventArgs.Arguments.Count} is incompatible; Expect : between {functionData.Info.MinArgumentLength} and {functionData.Info.MaxArgumentLength}\n{commandEventArgs.SocketUserMessage.Content}");
                }
            }
            else
            {
                throw new KeyNotFoundException($"{commandEventArgs.SocketUserMessage.Content}");
            }
        }
    }
}
