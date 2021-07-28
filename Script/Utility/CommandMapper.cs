using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PriconneBotConsoleApp.Attribute;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public static class CommandMapper
    {
        /// <summary>コマンドキャッシュのキーの型</summary>
        /// <param name="ChannelFeature">対応するチャンネル</param>
        /// <param name="Name">対応するコマンド名</param>
        private record CacheKey(ChannelFeatureType ChannelFeature, string Name);

        /// <summary>コマンドキャッシュの要素</summary>
        private class CacheValue
        {
            internal CacheValue(MethodInfo methodInfo)
            {
                Info = methodInfo.GetCustomAttribute<CommandAttribute>(false);

                if (Info != null)
                {
                    IsValid = true;
                    Handler = (Func<CommandEventArgs, Task>)Delegate.CreateDelegate(typeof(Func<CommandEventArgs, Task>), methodInfo);
                }
            }

            /// <summary>対象の<see cref="MethodInfo"/>に<see cref="CommandAttribute"/>がつけられているかどうか</summary>
            internal bool IsValid { get; }

            /// <summary>対象の<see cref="MethodInfo"/>につけられた<see cref="CommandAttribute"/></summary>
            internal CommandAttribute Info { get; }

            /// <summary>対象の<see cref="MethodInfo"/>の実体</summary>
            internal Func<CommandEventArgs, Task> Handler { get; }
        }

        private static ConcurrentDictionary<CacheKey, CacheValue> m_CommandCache;

        public static void InitCommandCache()
            => m_CommandCache = new(
                typeof(Commands)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Select(methodInfo => new CacheValue(methodInfo))
                    .Where(cacheValue => cacheValue.IsValid)
                    .SelectMany(
                        valueTuple => valueTuple.Info.CompatibleChannels.SelectMany(
                            _ => valueTuple.Info.Names,
                            (channelFeature, name) => new CacheKey(channelFeature, name)
                        ),
                        (valueTuple, keyTuple) => (keyTuple, valueTuple))
                    .ToDictionary(keyValueTuple => keyValueTuple.keyTuple, keyValueTuple => keyValueTuple.valueTuple)
            );

        public static async Task Invoke(CommandEventArgs commandEventArgs)
        {
            if (!m_CommandCache.TryGetValueMany(
                out var functionData,
                new(commandEventArgs.ChannelFeatureType, commandEventArgs.Name),
                new(ChannelFeatureType.All, commandEventArgs.Name),
                new(commandEventArgs.ChannelFeatureType, string.Empty)))
            {
                throw new KeyNotFoundException(commandEventArgs.SocketUserMessage.Content);
            }

            if (!functionData.Info.IsCompatibleArgumentLength(commandEventArgs.Arguments.Count))
            {
                throw new ArgumentException(
                    $"Argument length {commandEventArgs.Arguments.Count} is incompatible; Expect : between {functionData.Info.MinArgumentLength} and {functionData.Info.MaxArgumentLength}\n{commandEventArgs.SocketUserMessage.Content}"
                );
            }

            await functionData.Handler(commandEventArgs);
        }
    }
}
