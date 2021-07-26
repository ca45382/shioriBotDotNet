using System;
using System.ComponentModel;
using System.Reflection;
using PriconneBotConsoleApp.Attribute;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Extension
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum type)
        {
            var descriptionAttribute = type.GetType()
               .GetField(type.ToString()).GetCustomAttribute<DescriptionAttribute>(false);

            return descriptionAttribute?.Description ?? type.ToString();
        }

        public static MultiDescriptionData GetMultiDescripion(this Enum enumValue)
            => enumValue.GetType()
                .GetField(enumValue.ToString())
                .GetCustomAttribute<MultiDescriptionAttribute>(false)
                ?.Data
                ?? new MultiDescriptionData
                {
                    LongDescription = enumValue.ToString(),
                    ShortDescription = string.Empty,
                    Aliases = Array.Empty<string>(),
                };

        public static BossNumberType GetBossNumberType(this ChannelFeatureType channelFeatureType)
            => channelFeatureType switch
            {
                ChannelFeatureType.ProgressBoss1ID => BossNumberType.Boss1Number,
                ChannelFeatureType.ProgressBoss2ID => BossNumberType.Boss2Number,
                ChannelFeatureType.ProgressBoss3ID => BossNumberType.Boss3Number,
                ChannelFeatureType.ProgressBoss4ID => BossNumberType.Boss4Number,
                ChannelFeatureType.ProgressBoss5ID => BossNumberType.Boss5Number,
                _ => throw new ArgumentException($"Cannot cast to BossNumberType. {channelFeatureType}"),
            };
    }
}
