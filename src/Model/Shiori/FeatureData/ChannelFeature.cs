using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ShioriBot.Interface;

namespace ShioriBot.Model
{
    [Table("channel_feature")]
    public class ChannelFeature : IBotFeature
    {
        [Column("feature_id", TypeName = "INT UNSIGNED"), Key, Required]
        public uint FeatureID { get; set; }

        [Column("feature_name", TypeName = "varchar(20)")]
        public string FeatureName { get; set; }

        public List<ChannelData> ChannelData { get; set; }

    }
}
