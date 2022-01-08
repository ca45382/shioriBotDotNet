using System;
using ShioriBot.Net.DataType;
using ShioriBot.Net.Define;
using ShioriBot.Net.Model;

namespace ShioriBot.Net.Script
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

            var hpArgumentList = new uint[m_CommandEventArgs.Arguments.Count];
            var isValidArgument = true;

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
            var remainTime = 0u;
            var personNumber = 0;

            for (var i = 1; i < m_CommandEventArgs.Arguments.Count; i++)
            {
                if (bossHP <= hpArgumentList[i])
                {
                    remainTime = Calculate(bossHP, hpArgumentList[i]);
                    personNumber = i;

                    break;
                }

                bossHP -= hpArgumentList[i];
            }

            if (remainTime == 0)
            {
                m_CommandEventArgs.Channel.SendMessageAsync(EnumMapper.ToLabel(ErrorType.NotSubdueBoss));
                return;
            }

            var infomationComment = string.Format(EnumMapper.ToLabel(InformationType.CarryOverTimeResult), personNumber, remainTime);
            m_CommandEventArgs.Channel.SendMessageAsync(infomationComment);
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
