using System.Linq;
using ShioriBot.DataType;
using ShioriBot.Model;

namespace ShioriBot.Database
{
    public static class DatabaseMessageDataController
    {
        public static bool UpdateMessageID(ClanData clanData, ulong messageID, MessageFeatureType featureType)
        {
            using var databaseConnector = new ShioriDBContext();
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
            }

            databaseConnector.SaveChanges();
            transaction.Commit();

            return true;
        }
    }
}
