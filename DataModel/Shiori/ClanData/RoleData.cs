using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataModel
{
    [Table("role_data")]
    public class RoleData
    {
        [Column("id", TypeName = "BIGINT UNSIGNED AUTO_INCREMENT"), Key, Required]
        public ulong DataID { get; set; }

        [Column("clan_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong ClanID { get; set; }

        [Column("feature_id", TypeName = "INT UNSIGNED"), Required]
        public uint FeatureID { get; set; }

        [Column("channel_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong ChannelID { get; set; }
    }
}
