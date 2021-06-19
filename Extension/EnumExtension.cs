using System.ComponentModel;
using System.Reflection;

namespace PriconneBotConsoleApp.Extension
{
    public static class EnumExtension
    {
        public static string GetDescription(this System.Enum type)
        {
            var descriptionAttribute = type.GetType()
               .GetField(type.ToString()).GetCustomAttribute<DescriptionAttribute>(false);

            return descriptionAttribute?.Description;
        }
    }
}
