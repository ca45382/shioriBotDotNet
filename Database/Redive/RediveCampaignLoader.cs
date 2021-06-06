using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PriconneBotConsoleApp.DataModel;

namespace PriconneBotConsoleApp.Database
{
    class RediveCampaignLoader
    {
        public IEnumerable<CampaignData> LoadCampaignDatas()
        {
            using var rediveConnector = new RediveConnector();

            return rediveConnector.CampaignData
                .ToList();
        }
    }
}
