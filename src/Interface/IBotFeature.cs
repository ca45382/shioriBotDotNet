using System.Collections.Generic;

namespace ShioriBot.Net.Interface
{
    public interface IBotFeature
    {
        public uint FeatureID { get; set;} 

        public string FeatureName { get; set; }
    }

    public class IBotFeatureComparer : IEqualityComparer<IBotFeature>
    {
        public bool Equals(IBotFeature obj1, IBotFeature obj2)
        {
            if (obj1 == null || obj2 == null)
            {
                return false;
            }

            return obj1.FeatureID == obj2.FeatureID;
        }

        public int GetHashCode(IBotFeature botFeature)
        {
            //Check whether the object is null
            if (botFeature == null)
            {
                return 0;
            }

            //Get hash code for the Code field.
            return botFeature.FeatureID.GetHashCode();
        }
    }
}
