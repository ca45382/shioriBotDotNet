using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace PriconneBotConsoleApp
{
    public class Messages : ModuleBase
    {
        /* ルール列挙 */
        Dictionary<int, string> rule = new Dictionary<int, string>()
        {
            {0, "ナワバリ"},
            {1, "エリア"},
            {2, "ホコ"},
            {3, "ヤグラ"},
            {4, "アサリ"},
        };

        /* ステージ列挙 */
        Dictionary<int, string> stage = new Dictionary<int, string>()
        {
            {0,   "バッテラストリート" },
            {1 ,  "フジツボスポーツクラブ"},
            {2 ,  "ガンガゼ野外音楽堂"},
            {3 ,  "チョウザメ造船"},
            {4 ,  "海女美術大学"},
            {5 ,  "コンブトラック"},
            {6 ,  "マンタマリア号"},
            {7 ,  "ホッケふ頭"},
            {8 ,  "タチウオパーキング"},
            {9 , "エンガワ河川敷"},
            {10, "モズク農園"},
            {11,  "Ｂバスパーク"},
            {12,  "デボン海洋博物館"},
            {13,  "ザトウマーケット"},
            {14,  "ハコフグ倉庫"},
            {15,  "アロワナモール"}
        };

        /// <summary>
        /// [rl]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("rl")]
        public async Task rl()
        {


            Random random = new System.Random();
            int randomRule = random.Next(5);
            int randomStage = random.Next(16);

            string Messages = "次の試合のルールは、\n ・**" + rule[randomRule].ToString() + "**\n\n";
            Messages += "次の試合のステージは、\n ・**" + stage[randomStage].ToString() + "**\n";

            await ReplyAsync(Messages);
        }

    }
}