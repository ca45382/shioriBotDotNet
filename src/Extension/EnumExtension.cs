using System;
using System.ComponentModel;
using System.Reflection;
using ShioriBot.Attribute;
using ShioriBot.DataType;

namespace ShioriBot.Extension
{
    public static class EnumExtension
    {
        public static string GetDescription<T>(this T enumValue) where T : Enum
        {
            var enumString = enumValue.ToString();

            return typeof(T).GetField(enumString)?.GetCustomAttribute<DescriptionAttribute>(false)?.Description
                   ?? enumString;
        }

        public static MultiDescriptionData GetMultiDescription<T>(this T enumValue) where T : Enum
        {
            var enumString = enumValue.ToString();

            return typeof(T).GetField(enumString)?.GetCustomAttribute<MultiDescriptionAttribute>(false)?.Data
                   ?? new MultiDescriptionData
                   {
                       LongDescription = enumString,
                       ShortDescription = string.Empty,
                       Aliases = Array.Empty<string>()
                   };
        }

        public static BossNumberType GetBossNumberType(this ChannelFeatureType channelFeatureType)
            => channelFeatureType switch
            {
                ChannelFeatureType.ProgressBoss1ID => BossNumberType.Boss1Number,
                ChannelFeatureType.ProgressBoss2ID => BossNumberType.Boss2Number,
                ChannelFeatureType.ProgressBoss3ID => BossNumberType.Boss3Number,
                ChannelFeatureType.ProgressBoss4ID => BossNumberType.Boss4Number,
                ChannelFeatureType.ProgressBoss5ID => BossNumberType.Boss5Number,
                _ => throw new ArgumentException($"Cannot cast to BossNumberType. {channelFeatureType}")
            };
    }
}
