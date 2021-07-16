using System;

namespace PriconneBotConsoleApp.Attribute
{
    public struct MultiDescriptionData
    {
        public string LongDescription;
        public string ShortDescription;
        public string[] Aliases;
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
