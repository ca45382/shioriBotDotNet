using System;
using System.Collections.Generic;
using System.Linq;

namespace ShioriBot.Attribute
{
    public struct MultiDescriptionData
    {
        public string LongDescription { get; init; }
        public string ShortDescription { get; init; }
        public IReadOnlyList<string> Aliases { get; init; }

        private string[] m_Names;

        public string[] Names
            => m_Names ??= Aliases.Append(LongDescription).Append(ShortDescription).ToArray();
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class MultiDescriptionAttribute : System.Attribute
    {
        public MultiDescriptionAttribute(
            string longDescription,
            string shortDescription = null,
            params string[] aliases)
        {
            Data = new MultiDescriptionData
            {
                LongDescription = longDescription,
                ShortDescription = shortDescription,
                Aliases = aliases,
            };
        }

        public MultiDescriptionData Data { get; }
    }
}
