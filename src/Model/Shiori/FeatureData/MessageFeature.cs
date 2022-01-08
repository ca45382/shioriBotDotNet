using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ShioriBot.Net.Interface;

namespace ShioriBot.Net.Model
{
    [Table("message_feature")]
    public class MessageFeature : IBotFeature
    {
        [Column("feature_id", TypeName = "INT UNSIGNED"), Key, Required]
        public uint FeatureID { get; set; }

        [Column("feature_name", TypeName = "varchar(20)")]
        public string FeatureName { get; set; }

        public List<MessageData> MessageData { get; set; }
    }
}
