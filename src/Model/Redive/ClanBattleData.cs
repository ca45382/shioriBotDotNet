using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Model
{
    [Table("clan_battle_2_map_data")]
    public class ClanBattleData
    {
        [Column("id", TypeName = "INTEGER"), Key]
        public int ID { get; set; }

        [Column("clan_battle_id", TypeName = "INTEGER")]
        public int ClanBattleID { get; set; }

        [Column("lap_num_from", TypeName = "INTEGER")]
        public int LapNumberFrom { get; set; }

        [Column("lap_num_to", TypeName = "INTEGER")]
        public int LapNumberTo { get; set; }

        [Column("phase", TypeName = "INTEGER")]
        public int Phase { get; set; }

        [Column("wave_group_id_1", TypeName = "INTEGER")]
        public int WaveGroupID1 { get; set; }

        [Column("wave_group_id_2", TypeName = "INTEGER")]
        public int WaveGroupID2 { get; set; }

        [Column("wave_group_id_3", TypeName = "INTEGER")]
        public int WaveGroupID3 { get; set; }

        [Column("wave_group_id_4", TypeName = "INTEGER")]
        public int WaveGroupID4 { get; set; }

        [Column("wave_group_id_5", TypeName = "INTEGER")]
        public int WaveGroupID5 { get; set; }

        [Column("score_coefficient_1", TypeName = "REAL")]
        public float ScoreCoefficient1 { get; set; }

        [Column("score_coefficient_2", TypeName = "REAL")]
        public float ScoreCoefficient2 { get; set; }

        [Column("score_coefficient_3", TypeName = "REAL")]
        public float ScoreCoefficient3 { get; set; }

        [Column("score_coefficient_4", TypeName = "REAL")]
        public float ScoreCoefficient4 { get; set; }

        [Column("score_coefficient_5", TypeName = "REAL")]
        public float ScoreCoefficient5 { get; set; }

        public int GetWaveGroupID(BossNumberType bossNumber)
        {
            return bossNumber switch
            {
                BossNumberType.Boss1Number => WaveGroupID1,
                BossNumberType.Boss2Number => WaveGroupID2,
                BossNumberType.Boss3Number => WaveGroupID3,
                BossNumberType.Boss4Number => WaveGroupID4,
                BossNumberType.Boss5Number => WaveGroupID5,
                _ => 0,
            };
        }

        public float GetScoreCoefficient(BossNumberType bossNumber)
        {
            return bossNumber switch
            {
                BossNumberType.Boss1Number => ScoreCoefficient1,
                BossNumberType.Boss2Number => ScoreCoefficient2,
                BossNumberType.Boss3Number => ScoreCoefficient3,
                BossNumberType.Boss4Number => ScoreCoefficient4,
                BossNumberType.Boss5Number => ScoreCoefficient5,
                _ => 0,
            };
        }
    }
}
