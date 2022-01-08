using System;
using System.Collections.Generic;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Attribute
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CommandAttribute : System.Attribute
    {
        public CommandAttribute(
            string[] names = null,
            int minArgumentLength = 0,
            int maxArgumentLength = int.MaxValue,
            params ChannelFeatureType[] compatibleChannels)
        {
            Names = names ?? new[] { string.Empty };
            MinArgumentLength = minArgumentLength;
            MaxArgumentLength = maxArgumentLength;
            CompatibleChannels = compatibleChannels.Length == 0 ? new[] { ChannelFeatureType.All } : compatibleChannels;
        }

        public CommandAttribute(
            string name,
            int minArgumentLength = 0,
            int maxArgumentLength = int.MaxValue,
            params ChannelFeatureType[] compatibleChannels)
            : this(
                name == null ? null : new[] { name },
                minArgumentLength,
                maxArgumentLength,
                compatibleChannels)
        {
        }

        public CommandAttribute(
            AttackType attackType,
            int minArgumentLength = 0,
            int maxArgumentLength = int.MaxValue,
            params ChannelFeatureType[] compatibleChannels)
            : this(
                  attackType.GetMultiDescription().Names,
                  minArgumentLength,
                  maxArgumentLength,
                  compatibleChannels)
        {
        }

        /// <summary>
        /// 受け取ったコマンドを格納する
        /// </summary>
        public IReadOnlyList<string> Names { get; }

        /// <summary>
        /// 引数の長さの最小値
        /// </summary>
        public int MinArgumentLength { get; }

        /// <summary>
        /// 引数の長さの最大値
        /// </summary>
        public int MaxArgumentLength { get; }

        /// <summary>
        /// コマンドが対応するチャンネル
        /// </summary>
        public IReadOnlyList<ChannelFeatureType> CompatibleChannels { get; }

        public bool IsCompatibleArgumentLength(int argLength)
            => MinArgumentLength <= argLength && argLength <= MaxArgumentLength;
    }
}
