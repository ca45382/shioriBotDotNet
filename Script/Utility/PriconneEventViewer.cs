using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Script
{
    class PriconneEventViewer : BaseClass
    {
        private IMessage m_userMessage;
        public PriconneEventViewer(IMessage message)
        {
            m_userMessage = message;
        }

        public async Task SendEventInfomationByMessage()
        {
            if (m_userMessage.Content != "!today")
            {
                return;
            }

            var eventString = EventString();
            await m_userMessage.Channel.SendMessageAsync(text: eventString) ;
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
                IsInline = false,
                Name = "本日のキャンペーン",
                Value = todayCampaignString
            });

            embedBuilder.AddField(new EmbedFieldBuilder()
            {
                IsInline = false,
                Name = "明日のキャンペーン",
                Value = yesterdayCampaignString
            });

            return embedBuilder.Build();
        }
        public string CampaignLoader(DateTime nowTime)
        {
            var campaignAllData = new RediveCampaignLoader().LoadCampaignDatas(nowTime);
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
                    campaignSystemString + " " + campaignItemString + " " + campaignData.Value / 1000 + "倍"
                    );
            }

            campaignStringBuilder.AppendLine("```");

            return campaignStringBuilder.ToString();
        }
    }
}
