using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PriconneBotConsoleApp.DataModel;

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

            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            foreach (var programFeature in features)
            {
                // なぜかFeatureIDが0だと通らない
                if (databaseFeatures
                    .FirstOrDefault(x => x.FeatureID == programFeature.FeatureID) == null
                    && programFeature.FeatureID != 0)
                {
                    databaseConnector.ChannelFeatures.Add(programFeature);
                }
            }

            foreach (var databaseFeature in databaseFeatures)
            {
                // なぜかFeatureIDが0だと通らない
                if (features.FirstOrDefault(x => x.FeatureID == databaseFeature.FeatureID) == null
                    && databaseFeature.FeatureID != 0)
                {
                    databaseConnector.ChannelFeatures.Add(databaseFeature);
                }
            }

            databaseConnector.SaveChanges();
            transaction.Commit();
        }

        public void UpdateMessageFeature(IEnumerable<MessageFeature> features)
        {
            var databaseFeatures = LoadMessageFeature();

            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            foreach (var programFeature in features)
            {
                // なぜかFeatureIDが0だと通らない
                if (databaseFeatures
                    .FirstOrDefault(x => x.FeatureID == programFeature.FeatureID) == null
                    && programFeature.FeatureID != 0)
                {
                    databaseConnector.MessageFeatures.Add(programFeature);
                }
            }

            foreach (var databaseFeature in databaseFeatures)
            {
                // なぜかFeatureIDが0だと通らない
                if ( features.FirstOrDefault(x => x.FeatureID == databaseFeature.FeatureID) == null
                    && databaseFeature.FeatureID != 0)
                {
                    databaseConnector.MessageFeatures.Add(databaseFeature);
                }
            }

            databaseConnector.SaveChanges();
            transaction.Commit();
        }

        public void UpdateRoleFeature(IEnumerable<RoleFeature> features)
        {
            var databaseFeatures = LoadRoleFeature();

            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            foreach (var programFeature in features)
            {
                // なぜかFeatureIDが0だと通らない
                if (databaseFeatures
                    .FirstOrDefault(x => x.FeatureID == programFeature.FeatureID) == null
                    && programFeature.FeatureID != 0)
                {
                    databaseConnector.RoleFeatures.Add(programFeature);
                }
            }

            foreach (var databaseFeature in databaseFeatures)
            {
                // なぜかFeatureIDが0だと通らない
                if (features.FirstOrDefault(x => x.FeatureID == databaseFeature.FeatureID) == null
                    && databaseFeature.FeatureID != 0)
                {
                    databaseConnector.RoleFeatures.Add(databaseFeature);
                }
            }

            databaseConnector.SaveChanges();
            transaction.Commit();
        }
    }
}
