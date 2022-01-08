using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.Model
{
    [Table("message_data")]
    public class MessageData
    {
        [Column("id", TypeName = "BIGINT UNSIGNED AUTO_INCREMENT"), Key, Required]
        public ulong DataID { get; set; }

        [Column("message_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong MessageID { get; set; }

        [Column("update_time", TypeName = "TIMESTAMP on update CURRENT_TIMESTAMP")]
        [Timestamp]
        public DateTime UpdateTime { get; set; }

        //以下外部キー
        [Column("clan_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong ClanID { get; set; }

        [Column("feature_id", TypeName = "INT UNSIGNED"), Required]
        public uint FeatureID { get; set; }

        public ClanData ClanData { get; set; }
        public MessageFeature MessageFeature { get; set; }
    }
}
