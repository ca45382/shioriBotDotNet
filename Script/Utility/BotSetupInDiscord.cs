using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Script
{
    public class BotSetupInDiscord : BaseClass
    {

        private ServerData m_ServerData;
        private ClanData m_ClanData;

        public BotSetupInDiscord(ServerData serverData)
        {
            m_ServerData = serverData;
        }

        public BotSetupInDiscord(ClanData clanData)
        {
            m_ClanData = clanData;
        }

        public void RunByMessageData(SocketUserMessage message)
        {
            if (message.Content.StartsWith("$man"))
            {
                return;
            } 
            else if (!message.Content.StartsWith("$set"))
            {
                return;
            }

            

            return;
        }


        private void FeatureSettingMessageAnalyzer(SocketUserMessage userMessage)
        {
            var splitMessageContent = ZenToHan(userMessage.Content).Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if(splitMessageContent.Length != 3  || !uint.TryParse(splitMessageContent[2], out uint featureID))
            {
                return;
            }

            // メモ
            // <@&0000> ロールIDの形
            // <#0000> チャンネルの形
            if (Regex.IsMatch(splitMessageContent[1], @"<#\d+>"))
            {
                if (Enum.GetValues(typeof(ChannelFeatureType)).Cast<ChannelFeatureType>().Contains((ChannelFeatureType)featureID))
                {

                }
            }
            else if (Regex.IsMatch(splitMessageContent[1], @"<@&\d+>"))
            {
                
            }
            else
            {
                return;
            }


        }

        private async Task SendManualMessage()
        {

            return;
        }

        /// <summary>
        /// Adminのロールを追加・削除が可能
        /// </summary>
        private void ModifyAdminRole()
        {

        }

        private void ModifyClanRole()
        {

        }

        private void ModifyChannel()
        {

        }

        private void ModifyRole()
        {

        }
    }
}
