using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        public IEnumerable<CampaignData> LoadCampaignDatas(DateTime dateTime)
        {
            using var rediveConnector = new RediveConnector();
            
            var aaa = rediveConnector.CampaignData.AsEnumerable()
                .Where(b => b.CampaignStartTime <= dateTime
                    && dateTime <= b.CampaignEndTime)
                .ToList();

            /*
            var aaa = rediveConnector.CampaignData.AsQueryable()
                .Where(b => DbFunctions.( b.CampaignStartTime <= dateTime
                    && dateTime <= b.CampaignEndTime)
                .AsEnumerable();
            */
            return aaa;
        }
    }
}
