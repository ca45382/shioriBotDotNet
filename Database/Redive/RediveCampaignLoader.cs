using System;
using System.Collections.Generic;
using System.Linq;
using PriconneBotConsoleApp.Model;

namespace PriconneBotConsoleApp.Database
{
    public static class RediveCampaignLoader
    {
        public static IEnumerable<CampaignData> LoadCampaignData()
        {
            using var rediveConnector = new RediveConnector();

            return rediveConnector.CampaignData.ToList();
        }

        public static IEnumerable<CampaignData> LoadCampaignData(DateTime dateTime)
        {
            using var rediveConnector = new RediveConnector();

            return rediveConnector.CampaignData.AsEnumerable()
                .Where(b => b.CampaignStartTime <= dateTime && dateTime <= b.CampaignEndTime)
                .ToList();
        }
    }
}
