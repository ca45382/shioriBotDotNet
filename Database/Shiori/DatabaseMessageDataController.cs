﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Database
{
    public class DatabaseMessageDataController
    {
        public bool UpdateMessageID(ClanData clanData, ulong messageID, MessageFeatureType featureType)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            var clanID = databaseConnector.ClanData
                .FirstOrDefault(d => d.ServerID == clanData.ServerID && d.ClanRoleID == clanData.ClanRoleID)
                ?.ClanID ?? 0;

            if (clanID == 0)
            {
                transaction.Rollback();
                return false;
            }

            var updateData = databaseConnector.MessageData
                .FirstOrDefault(d => d.ClanID == clanID && d.FeatureID == (uint)featureType);

            if (updateData == null)
            {
                databaseConnector.MessageData.Add(
                    new MessageData()
                    {
                        ClanID = clanID,
                        FeatureID = (uint)featureType,
                        MessageID = messageID,
                    });
            }
            else
            {
                updateData.MessageID = messageID;
                databaseConnector.SaveChanges();
            }

            transaction.Commit();

            return true;
        }
    }
}