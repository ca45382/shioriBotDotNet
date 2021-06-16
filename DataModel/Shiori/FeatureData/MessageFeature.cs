using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataModel
{
    [Table("message_feature")]
    public class MessageFeature
    {

        [Column("feature_id", TypeName = "INT UNSIGNED"), Key, Required]
        public uint FeatureID { get; set; }

        [Column("feature_name", TypeName = "varchar(20)")]
        public string FeatureName { get; set; }

    }
}
