using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Define;

namespace PriconneBotConsoleApp.Script
{
    public class Program
    {
        private DiscordSocketClient m_client;
        private DiscordSocketConfig m_config;

        private readonly static string ConfigPath = Path.Combine("data", "botConfig.json");
        static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        /// <summary>
        /// ボットの起動処理
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
            BotConfigManager.SetJsonConfig(ConfigPath);
            CommandMapper.InitCommandCache();

            m_config = new DiscordSocketConfig
            {
                MessageCacheSize = 10,
                GatewayIntents = GatewayIntents.All,
            };

            var initialize = new BotInitialize();
            RediveClanBattleData.ReloadData();
            initialize.UpdateRediveDatabase();

            m_client = new DiscordSocketClient(m_config);
            m_client.MessageReceived += CommandRecieved;
            m_client.GuildMembersDownloaded += GuildMembersDownloaded;
            m_client.UserLeft += UserLeft;
            m_client.GuildMemberUpdated += GuildMemberUpdated;
            m_client.InteractionCreated += InteractionCreated;
            m_client.Log += Log;

            var commands = new CommandService();
            var services = new ServiceCollection().BuildServiceProvider();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            await m_client.LoginAsync(TokenType.Bot, BotConfigManager.Token);
            await m_client.StartAsync();
            await RefreshInUpdateDate();
            await Task.Delay(-1);
        }

        /// <summary>
        /// メッセージの受信処理
        /// </summary>
        /// <param name="socketMessage"></param>
        /// <returns></returns>
        private async Task CommandRecieved(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage socketUserMessage)
            {
                return;
            }

            Console.WriteLine($"{socketUserMessage.Channel.Name} {socketUserMessage.Author.Username}:{socketUserMessage}");

            if (socketUserMessage.Author.IsBot)
            {
                return;
            }

            try
            {
                await CommandMapper.Invoke(new CommandEventArgs(socketUserMessage));
            }
            catch 
            {
            }
        }

        /// <summary>
        /// 起動時にサーバー情報がbotにダウンロードされた際に
        /// 動作する。
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        private Task GuildMembersDownloaded(SocketGuild guild)
        {
            var discordDataLoader = new DiscordDataLoader();
            discordDataLoader.UpdateServerData(guild);
            discordDataLoader.UpdateClanData(guild);
            discordDataLoader.UpdatePlayerData(guild);
            return Task.CompletedTask;
        }

        private Task UserLeft(SocketGuildUser userInfo)
        {
            var discordDataLoader = new DiscordDataLoader();
            discordDataLoader.UpdateServerData(userInfo.Guild);
            discordDataLoader.UpdateClanData(userInfo.Guild);
            discordDataLoader.UpdatePlayerData(userInfo.Guild);
            return Task.CompletedTask;
        }

        private Task GuildMemberUpdated(Cacheable<SocketGuildUser, ulong> cachedGuildUser, SocketGuildUser newUserInfo)
        {
            var discordDataLoader = new DiscordDataLoader();
            discordDataLoader.UpdateServerData(newUserInfo.Guild);
            discordDataLoader.UpdateClanData(newUserInfo.Guild);
            discordDataLoader.UpdatePlayerData(newUserInfo.Guild);
            return Task.CompletedTask;
        }

        private async Task InteractionCreated(SocketInteraction socketInteraction)
        {
            if (socketInteraction.Type != InteractionType.MessageComponent)
            {
                return;
            }

            await new ReceiveInteractionController(socketInteraction).Run();
        }

        private async Task RefreshInUpdateDate()
        {
            DateTime nowDateTime;
            TimeSpan nowTime;
            TimeSpan updateTimeSpan;

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
                await new UpdateDate(m_client).DeleteYesterdayData();
            }
        }

        /// <summary>
        /// コンソール表示処理
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
