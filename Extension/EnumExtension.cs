using PriconneBotConsoleApp.Attribute;
using System;
using System.ComponentModel;
using System.Reflection;

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
    }
}
