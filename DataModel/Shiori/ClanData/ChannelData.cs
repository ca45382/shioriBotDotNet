using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataModel
{
    [Table("channel_data")]
    public class ChannelData
    {
        [Column("id", TypeName = "BIGINT UNSIGNED AUTO_INCREMENT"), Key, Required]
        public ulong DataID { get; set; }

        [Column("channel_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong ChannelID { get; set; }

        //以下外部キー
        [Column("clan_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong ClanID { get; set; }

        [Column("feature_id", TypeName = "INT UNSIGNED"), Required]
        public uint FeatureID { get; set; }

        public ClanData ClanData { get; set; }
        public ChannelFeature ChannelFeature { get; set; }
    }
}
