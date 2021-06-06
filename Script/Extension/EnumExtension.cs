using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

//using PriconneBotConsoleApp.Enum;

namespace PriconneBotConsoleApp.Script
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
