using System;
using System.Collections.Generic;
using System.Linq;
using ShioriBot.Model;

namespace ShioriBot.Database
{
    public static class RediveCampaignLoader
    {
        public static IEnumerable<CampaignData> LoadCampaignData()
        {
            using var rediveConnector = new RediveDBContext();

            return rediveConnector.CampaignData.ToList();
        }

        public static IEnumerable<CampaignData> LoadCampaignData(DateTime dateTime)
        {
            using var rediveConnector = new RediveDBContext();

            return rediveConnector.CampaignData.AsEnumerable()
                .Where(b => b.CampaignStartTime <= dateTime && dateTime <= b.CampaignEndTime)
                .ToList();
        }
    }
}
