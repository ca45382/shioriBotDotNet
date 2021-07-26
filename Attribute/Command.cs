using System;
using System.Collections.Generic;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Attribute
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : System.Attribute
    {
        public CommandAttribute(
            int minArgumentLength = 0,
            int maxArgumentLength = int.MaxValue,
            string[] commandText = null,
            params ChannelFeatureType[] channelFeatureType
           )
        {
            Names = commandText ?? new[] { string.Empty };
            ChannelTypes = channelFeatureType.Length == 0 ? new[] { ChannelFeatureType.All } : channelFeatureType;

            MinArgumentLength = minArgumentLength;
            MaxArgumentLength = maxArgumentLength;
        }

        /// <summary>
        /// 受け取ったコマンドを格納する
        /// </summary>
        public IReadOnlyList<string> Names { get; }

        /// <summary>
        /// 受け取ったコマンドが発信されたチャンネル
        /// </summary>
        public IReadOnlyList<ChannelFeatureType> ChannelTypes { get; }

        /// <summary>
        /// 引数の長さの最小値
        /// </summary>
        public int MinArgumentLength { get; }

        /// <summary>
        /// 引数の長さの最大値
        /// </summary>
        public int MaxArgumentLength { get; }

        public bool IsCompatibleArgumentLength(int argLength)
            => (MinArgumentLength <= argLength && argLength <= MaxArgumentLength);
    }
}
