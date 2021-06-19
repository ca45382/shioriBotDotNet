using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataModel
{
    [Table("role_data")]
    public class RoleData
    {
        [Column("id", TypeName = "BIGINT UNSIGNED AUTO_INCREMENT"), Key, Required]
        public ulong DataID { get; set; }

        [Column("role_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong RoleID { get; set; }

        [Column("clan_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong ClanID { get; set; }

        [Column("feature_id", TypeName = "INT UNSIGNED"), Required]
        public uint FeatureID { get; set; }

        public ClanData ClanData { get; set; }
        public RoleFeature RoleFeature { get; set; }
    }
}
