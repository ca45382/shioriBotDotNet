using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using PriconneBotConsoleApp.Attribute;
using PriconneBotConsoleApp.DataType;
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

        private readonly static Dictionary<(Type, int), string> m_SingleDescriptionDictionary = new();
        private readonly static Dictionary<(Type, int), MultiDescriptionData> m_MultiDescriptionDictionary = new();

        public static string GetString<T>(T data) where T : Enum
            => m_DescriptionTypeDictionary.GetValueOrInitialize(typeof(T), GetDescriptionType<T>) switch
            {
                DescriptionType.Single => GetSingleString(data),
                DescriptionType.Multi => GetLongString(data),
                _ => throw new InvalidProgramException(),
            };

        private static string GetSingleString<T>(T data) where T : Enum
            => m_SingleDescriptionDictionary.GetValueOrInitialize((typeof(T), (int)(dynamic)data), data.GetDescription);

        public static string GetLongString<T>(T data) where T : Enum
            => m_MultiDescriptionDictionary.GetValueOrInitialize((typeof(T), (int)(dynamic)data), data.GetMultiDescripion).LongDescription;

        public static string GetShortString<T>(T data) where T : Enum
            => m_MultiDescriptionDictionary.GetValueOrInitialize((typeof(T), (int)(dynamic)data), data.GetMultiDescripion).ShortDescription;

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
    }
}
