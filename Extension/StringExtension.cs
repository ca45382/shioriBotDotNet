using System.Text.RegularExpressions;

namespace PriconneBotConsoleApp.Extension
{
    public static class StringExtension
    {
        public static string ZenToHan(this string textData)
        {
            var convertText = textData;
            convertText = convertText.Replace('　', ' ');
            convertText = convertText.Replace('＠', '@');
            convertText = convertText.Replace('！', '!');
            convertText = Regex.Replace(convertText, "[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString());
            convertText = Regex.Replace(convertText, "[ａ-ｚ]", p => ((char)(p.Value[0] - 'ａ' + 'a')).ToString());
            convertText = Regex.Replace(convertText, "[Ａ-Ｚ]", p => ((char)(p.Value[0] - 'Ａ' + 'A')).ToString());
            return convertText;
        }
    }
}
