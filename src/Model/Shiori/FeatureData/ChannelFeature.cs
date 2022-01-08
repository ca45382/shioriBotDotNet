﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ShioriBot.Net.Interface;

namespace ShioriBot.Net.Model
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
