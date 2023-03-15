using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShioriBot.Define;
using ShioriBot.Model;

namespace ShioriBot.Script
{
    public class Program
    {
        //old code
        private readonly static string ConfigPath = Path.Combine("data", "botConfig.json");

        private readonly IConfiguration m_Configuration;
        private readonly IServiceProvider m_Service;

        private static void Main(string[] args) 
            => new Program().MainAsync(args)
            .GetAwaiter()
            .GetResult();

        private Program()
        {
            // 設定ファイルを開く
            m_Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + @"\data")
                .AddJsonFile("botConfig.json")
                .Build();

            var socketConfig = new DiscordSocketConfig()
            {
                MessageCacheSize = 0xFF,
                GatewayIntents = (GatewayIntents)0b0_1000_0011_0000_0011,
                //GatewayIntents = GatewayIntents.All,
            };

            // サービス
            m_Service = new ServiceCollection()
                .AddSingleton(m_Configuration)
                .AddSingleton(socketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<Handler>()
                .BuildServiceProvider();
        }

        /// <summary>
        /// ボットの起動処理
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync(string[] args)
        {
            BotConfigManager.SetJsonConfig(ConfigPath);
            CommandMapper.InitCommandCache();

            var initialize = new BotInitialize();
            RediveClanBattleData.ReloadData();
            initialize.UpdateRediveDatabase();

            var client = m_Service.GetRequiredService<DiscordSocketClient>();

            await m_Service.GetRequiredService<Handler>()
                .InitializeAsync();
            
            await client.LoginAsync(TokenType.Bot, m_Configuration["discord:token"]);
            await client.StartAsync();
            await RefreshInUpdateDate();
            await Task.Delay(-1);
        }

        private async Task RefreshInUpdateDate()
        {
            DateTime nowDateTime;
            TimeSpan nowTime;
            TimeSpan updateTimeSpan;

            var client = m_Service.GetRequiredService<DiscordSocketClient>();
            var initialize = new BotInitialize();
            var updateTime = TimeDefine.DailyRefreshTime;

            while (true)
            {
                nowDateTime = DateTime.Now;
                nowTime = nowDateTime.TimeOfDay;

                if (updateTime - nowTime < new TimeSpan(0, 0, 0))
                {
                    updateTimeSpan = updateTime - nowTime + new TimeSpan(1, 0, 0, 0);
                }
                else
                {
                    updateTimeSpan = updateTime - nowTime;
                }

                await Task.Run(() => Thread.Sleep(updateTimeSpan));
                initialize.UpdateRediveDatabase();
                RediveClanBattleData.ReloadData();
                await new UpdateDate(client).DeleteYesterdayData();
            }
        }
    }
}
