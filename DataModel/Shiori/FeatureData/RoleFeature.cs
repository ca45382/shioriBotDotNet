using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using PriconneBotConsoleApp.Interface;

namespace PriconneBotConsoleApp.DataModel
{
    [Table("role_feature")]
    public class RoleFeature : IBotFeature 
    {
        [Column("feature_id", TypeName = "INT UNSIGNED"), Key, Required]
        public uint FeatureID { get; set; }

        [Column("feature_name", TypeName = "varchar(20)")]
        public string FeatureName { get; set; }

        public List<RoleData> RoleData { get; set; }

    }
}
