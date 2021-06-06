using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.Enum;

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
