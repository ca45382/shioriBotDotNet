using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using PriconneBotConsoleApp.Attribute;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public static class EnumMapper
    {
        private enum DescriptionType
        {
            Single,
            Multi,
        }

        private readonly static Dictionary<Type, DescriptionType> m_DescriptionTypeDictionary = new();

        private readonly static Dictionary<(Type, Enum), string> m_SingleDescriptionDictionary = new();
        private readonly static Dictionary<(Type, Enum), MultiDescriptionData> m_MultiDescriptionDictionary = new();
        private readonly static Dictionary<string, Enum> m_ParseCache = new();


        public static string ToLabel<T>(this T data) where T : Enum
            => m_DescriptionTypeDictionary.GetValueOrInitialize(typeof(T), GetDescriptionType<T>) switch
            {
                DescriptionType.Single => ToSingleLabel(data),
                DescriptionType.Multi => ToLongLabel(data),
                _ => throw new InvalidProgramException(),
            };

        private static MultiDescriptionData GetMultiDescriptionData<T>(T data) where T : Enum
            => m_MultiDescriptionDictionary.GetValueOrInitialize((typeof(T), data), data.GetMultiDescripion);

        private static string ToSingleLabel<T>(this T data) where T : Enum
            => m_SingleDescriptionDictionary.GetValueOrInitialize((typeof(T), data), data.GetDescription);

        public static string ToLongLabel<T>(this T data) where T : Enum
            => GetMultiDescriptionData(data).LongDescription;

        public static string ToShortLabel<T>(this T data) where T : Enum
            => GetMultiDescriptionData(data).ShortDescription;

        private static string[] GetAliases<T>(this T data) where T : Enum
            => GetMultiDescriptionData(data).Aliases;

        private static DescriptionType GetDescriptionType<T>() where T : Enum
        {
            foreach (T member in typeof(T).GetEnumValues())
            {
                var field = typeof(T).GetField(member.ToString());
                if (field.GetCustomAttribute<DescriptionAttribute>(false) != null)
                {
                    return DescriptionType.Single;
                }

                if (field.GetCustomAttribute<MultiDescriptionAttribute>(false) != null)
                {
                    return DescriptionType.Multi;
                }
            }

            return DescriptionType.Single;
        }

        public static T Parse<T>(string input) where T : Enum
            => (T)m_ParseCache.GetValueOrInitialize(
                input,
                () => typeof(T).GetEnumValues().Cast<T>().First(x => x.IsMatched(input))
            );

        public static bool IsMatched<T>(this T value, string input) where T : Enum
            =>value.ToLabel() == input
                || value.ToShortLabel() == input
                || value.GetAliases().Any(x => x == input);

        public static bool TryParse<T>(string input, out T result) where T : Enum
        {
            try
            {
                result = Parse<T>(input);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
