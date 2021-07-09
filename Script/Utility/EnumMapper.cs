using System;
using System.Collections.Generic;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public class EnumMapper
    {
        private Dictionary<Type, Dictionary<int, string>> m_EnumDictionary = new Dictionary<Type, Dictionary<int, string>>();

        private Dictionary<int, string> m_ReactionTypes = new Dictionary<int, string>();
        private Dictionary<int, string> m_ErrorTypes = new Dictionary<int, string>();

        private static EnumMapper s_Instance;
        public static EnumMapper I => s_Instance ??= new EnumMapper();

        public EnumMapper()
        {
            m_EnumDictionary.Add(typeof(ReactionType), m_ReactionTypes);
            m_EnumDictionary.Add(typeof(ErrorType), m_ErrorTypes);
        }

        public string GetString<T>(T data) where T : Enum
        {
            if (data == null)
            {
                return null;
            }

            var dictionaryData = m_EnumDictionary[typeof(T)];
            var castEnum = Enum.Parse(typeof(T), data.ToString()) as Enum;
            var castInt = Convert.ToInt32(castEnum);

            if (dictionaryData.ContainsKey(castInt))
            {
                return dictionaryData[castInt];
            }
            else
            {
                var descriptionString = castEnum.GetDescription();
                dictionaryData[castInt] = descriptionString;
                return descriptionString;
            }
        }
    }
}
