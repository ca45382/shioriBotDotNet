using System;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Define;
using PriconneBotConsoleApp.Model;

namespace PriconneBotConsoleApp.Script
{
    public class CarryOverTimeCalculator
    {
        private readonly CommandEventArgs m_CommandEventArgs;

        public CarryOverTimeCalculator(CommandEventArgs commandEventArgs)
            => m_CommandEventArgs = commandEventArgs;

        public void Run()
        {
            if (m_CommandEventArgs == null)
            {
                return;
            }

            uint[] hpArgumentList = new uint[m_CommandEventArgs.Arguments.Count];
            bool isValidArgument = true;

            for (var i = 0; i < m_CommandEventArgs.Arguments.Count; i++)
            {
                if (!uint.TryParse(m_CommandEventArgs.Arguments[i], out uint timeTemp))
                {
                    isValidArgument = false;
                    break;
                }

                hpArgumentList[i] = timeTemp;
            }

            // 1つでも自然数でない引数があった場合エラーになる
            if (!isValidArgument)
            {
                m_CommandEventArgs.Channel.SendMessageAsync(EnumMapper.ToLabel(ErrorType.InvalidDamage));
                return;
            }

            // いくつかの変数を受け取って持ち越し秒数を計算する
            // のちに拡張する予定

            var bossHP = hpArgumentList[0];
            uint remainTime = 0;
            int personNumber = 0;

            for (var i = 1; i < m_CommandEventArgs.Arguments.Count; i++)
            {
                if(bossHP <= hpArgumentList[i])
                {
                    remainTime = Calculate(bossHP, hpArgumentList[i]);
                    personNumber = i;

                    break;
                }

                bossHP -= hpArgumentList[i];
            }

            if(remainTime == 0)
            {
                m_CommandEventArgs.Channel.SendMessageAsync(EnumMapper.ToLabel(ErrorType.NotSubdueBoss));
                return;
            }

            m_CommandEventArgs.Channel.SendMessageAsync(string.Format(
                EnumMapper.ToLabel(InformationType.CarryOverTimeResult),
                personNumber,
                remainTime                
                ));
        }

        private static uint Calculate(uint remainBossHP, uint attackDamage, uint remainTime = 0)
        {
            if (!CommonDefine.IsValidDamageValue((int)attackDamage)
                || remainBossHP > attackDamage
                || remainTime > CommonDefine.MaxBattleTime
                || 0 > remainTime)
            {
                return 0;
            }

            var carryOverTime = (uint)Math.Ceiling((1 - ((double)remainBossHP / attackDamage)) * 90 + 20);

            return carryOverTime > CommonDefine.MaxBattleTime ? CommonDefine.MaxBattleTime : carryOverTime;
        }
    }
}
