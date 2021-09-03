using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.Model;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public class PriconneEventViewer
    {
        private CommandEventArgs m_CommandEventArgs;
        public PriconneEventViewer(CommandEventArgs commandEventArgs)
            => m_CommandEventArgs = commandEventArgs;

        public async Task SendEventInfomationByMessage()
        {
            var eventString = EventString();
            await m_CommandEventArgs.Channel.SendMessageAsync(text: eventString) ;
        }

        public string EventString()
        {
            var nowTime = DateTime.Now;

            var eventStringBuilder = new StringBuilder();
            eventStringBuilder.AppendLine("本日のキャンペーン");
            eventStringBuilder.AppendLine(CampaignLoader(nowTime));

            return eventStringBuilder.ToString();
        }

        public Embed EventEmbed()
        {
            var nowTime = DateTime.Now;
            var todayCampaignString = CampaignLoader(nowTime);
            var yesterdayCampaignString = CampaignLoader(nowTime.AddDays(1));

            var embedBuilder = new EmbedBuilder
            {
                Title = "本日のキャンペーン"
            };

            embedBuilder.AddField(new EmbedFieldBuilder()
            {
                Name = "本日のキャンペーン",
                Value = todayCampaignString
            });

            embedBuilder.AddField(new EmbedFieldBuilder()
            {
                Name = "明日のキャンペーン",
                Value = yesterdayCampaignString
            });

            return embedBuilder.Build();
        }

        public string CampaignLoader(DateTime nowTime)
        {
            var campaignAllData = RediveCampaignLoader.LoadCampaignData(nowTime);
            campaignAllData = campaignAllData
                .OrderBy(b => b.IconImage);

            var campaignStringBuilder = new StringBuilder();
            campaignStringBuilder.AppendLine("```Python");

            foreach(var campaignData in campaignAllData)
            {
                var campaignSystemType = CampaignSystemType.Unknown;
                var campaignIconType = CampaignIconType.Unknown;

                if (Enum.IsDefined(typeof(CampaignSystemType), campaignData.SystemID))
                {
                    campaignSystemType = (CampaignSystemType)campaignData.SystemID;
                }

                if (Enum.IsDefined(typeof(CampaignIconType), campaignData.IconImage))
                {
                    campaignIconType = (CampaignIconType)campaignData.IconImage;
                }
                
                var campaignSystemString = campaignSystemType.GetDescription();
                var campaignItemString = campaignIconType.GetDescription();

                campaignStringBuilder.AppendLine(
                    campaignSystemString + " " + campaignItemString + " " + campaignData.CampaignValue / 1000 + "倍"
                    );
            }

            campaignStringBuilder.AppendLine("```");

            return campaignStringBuilder.ToString();
        }
    }
}
