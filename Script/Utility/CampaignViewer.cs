using System;
using System.Linq;
using System.Text;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Script
{
    class CampaignViewer
    {
        public void TodayCampaignLoader()
        {
            var campaignData = new RediveCampaignLoader().LoadCampaignDatas();

            var data = campaignData.ToArray().Last();
            var exportString = new StringBuilder();

            var campaignSystemType = CampaignSystemType.Unknown;

            if (System.Enum.IsDefined(typeof(CampaignSystemType), data.SystemID))
            {
                campaignSystemType = (CampaignSystemType) data.SystemID;
            }

            var aa = campaignSystemType.GetDescription();
            Console.WriteLine(aa);
        }
    }
}
