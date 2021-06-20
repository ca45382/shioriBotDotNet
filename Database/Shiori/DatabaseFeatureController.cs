using System.Collections.Generic;
using System.Linq;

using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Interface;

namespace PriconneBotConsoleApp.Database
{
    public class DatabaseFeatureController
    {
        public IEnumerable<ChannelFeature> LoadChannelFeature()
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.ChannelFeatures.ToList();
        }

        public IEnumerable<MessageFeature> LoadMessageFeature()
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.MessageFeatures.ToList();
        }

        public IEnumerable<RoleFeature> LoadRoleFeature()
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.RoleFeatures.ToList();
        }

        public void UpdateChannelFeature(IEnumerable<ChannelFeature> features)
        {
            var databaseFeatures = LoadChannelFeature();
            var createFeatures = GetCreateData(features, databaseFeatures);
            var removeFeatures = GetRemoveData(features, databaseFeatures);

            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            foreach (var createFeature in createFeatures)
            {
                databaseConnector.ChannelFeatures.Add((ChannelFeature)createFeature);
            }

            foreach (var removeFeature in removeFeatures)
            {
                databaseConnector.ChannelFeatures.Remove((ChannelFeature)removeFeature);
            }

            databaseConnector.SaveChanges();
            transaction.Commit();
        }

        public void UpdateMessageFeature(IEnumerable<MessageFeature> features)
        {
            var databaseFeatures = LoadMessageFeature();
            var createFeatures = GetCreateData(features, databaseFeatures);
            var removeFeatures = GetRemoveData(features, databaseFeatures);

            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            foreach (var createFeature in createFeatures)
            {
                databaseConnector.MessageFeatures.Add((MessageFeature)createFeature);
            }

            foreach (var removeFeature in removeFeatures)
            {
                databaseConnector.MessageFeatures.Remove((MessageFeature)removeFeature);
            }

            databaseConnector.SaveChanges();
            transaction.Commit();
        }

        public void UpdateRoleFeature(IEnumerable<RoleFeature> features)
        {
            var databaseFeatures = LoadRoleFeature();
            var createFeatures = GetCreateData(features, databaseFeatures);
            var removeFeatures = GetRemoveData(features, databaseFeatures);

            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            foreach (var createFeature in createFeatures)
            {
                databaseConnector.RoleFeatures.Add((RoleFeature)createFeature);
            }

            foreach (var removeFeature in removeFeatures)
            {
                databaseConnector.RoleFeatures.Remove((RoleFeature)removeFeature);
            }

            databaseConnector.SaveChanges();
            transaction.Commit();
        }

        private IEnumerable<IBotFeature> GetCreateData(IEnumerable<IBotFeature> programFeatures, IEnumerable<IBotFeature> databaseFeatures)
        {
            var updateFeatures = new List<IBotFeature>();

            foreach (var programFeature in programFeatures)
            {
                // なぜかFeatureIDが0だと通らない
                if (databaseFeatures.FirstOrDefault(x => x.FeatureID == programFeature.FeatureID) == null
                    && programFeature.FeatureID != 0)
                {
                    updateFeatures.Add(programFeature);
                }
            }

            return updateFeatures;
        }

        private IEnumerable<IBotFeature> GetRemoveData(IEnumerable<IBotFeature> programFeatures, IEnumerable<IBotFeature> databaseFeatures)
        {
            var removeFeatures = new List<IBotFeature>();

            foreach (var databaseFeature in databaseFeatures)
            {
                // GetCreateDataと同様の理由
                // なぜかFeatureIDが0だと通らない
                if (programFeatures.FirstOrDefault(x => x.FeatureID == databaseFeature.FeatureID) == null 
                    && databaseFeature.FeatureID != 0)
                {
                    removeFeatures.Add(databaseFeature);
                }
            }

            return removeFeatures;
        }
    }
}
