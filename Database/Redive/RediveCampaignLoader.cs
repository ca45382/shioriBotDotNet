using System;
using System.Collections.Generic;
using System.Linq;
using PriconneBotConsoleApp.DataModel;

namespace PriconneBotConsoleApp.Database
{
    public static class RediveCampaignLoader
    {
        public static IEnumerable<CampaignData> LoadCampaignDatas()
        {
            using var rediveConnector = new RediveConnector();

            return rediveConnector.CampaignData.ToList();
        }

        public static IEnumerable<CampaignData> LoadCampaignDatas(DateTime dateTime)
        {
            using var rediveConnector = new RediveConnector();

            return rediveConnector.CampaignData.AsEnumerable()
                .Where(b => b.CampaignStartTime <= dateTime && dateTime <= b.CampaignEndTime)
                .ToList();
        }
    }
}
