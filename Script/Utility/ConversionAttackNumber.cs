using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Script
{
    public static class ConversionAttackNumber
    {
        public static int StringToAttackNumber(string inputString)
        {
            return inputString switch
            {
                "物理" or "物" or "b" => 1,
                "魔法" or "魔" or "m" => 2,
                "ニャル" or "ニ" or "n" => 3,
                "持ち越し" or "持越し" or "-" => 99,
                _ => 0,
            };
        }

        public static string AttackNumberToString(int inputInt)
        {
            return inputInt switch
            {
                1 => "物理",
                2 => "魔法",
                3 => "ニャル",
                99 => "持ち越し",
                _ => "不明",
            };
        }

        public static string AttackNumberToShortString(int inputInt)
        {
            return inputInt switch
            {
                1 => "物",
                2 => "魔",
                3 => "ニ",
                99 => "持",
                _ => "不",
            };
        }
    }
}
