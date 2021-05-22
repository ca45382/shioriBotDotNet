﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Script
{
    class Program
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

            var jsonSettingData = new JsonDataManager(ConfigPath);

            m_config = new DiscordSocketConfig
            {
                MessageCacheSize = 10
            };

            var initialize = new BotInitialize();
            initialize.UpdateRediveDatabase();

            m_client = new DiscordSocketClient(m_config);
            m_client.MessageReceived += CommandRecieved;
            m_client.GuildMembersDownloaded += GuildMembersDownloaded;
            m_client.UserLeft += UserLeft;
            m_client.GuildMemberUpdated += GuildMemberUpdated;
            m_client.ReactionAdded += ReactionAdded;
            m_client.Log += Log;

            var commands = new CommandService();
            var services = new ServiceCollection().BuildServiceProvider();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            await m_client.LoginAsync(TokenType.Bot, jsonSettingData.Token);
            await m_client.StartAsync();
            await Test();
            await Task.Delay(-1);
        }

        /// <summary>
        /// メッセージの受信処理
        /// </summary>
        /// <param name="messageParam"></param>
        /// <returns></returns>
        private async Task CommandRecieved(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message))
            {
                return;
            }

            Console.WriteLine($"{message.Channel.Name} {message.Author.Username}:{message}");

            if (message.Author.IsBot)
            {
                return;
            }

            var receiveMessages = new ReceiveMessageController(message);
            await receiveMessages.RunMessageReceive();
        }

        /// <summary>
        /// 起動時にサーバー情報がbotにダウンロードされた際に
        /// 動作する。
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        private Task GuildMembersDownloaded(SocketGuild guild)
        {
            new PlayerDataLoader().UpdatePlayerData(guild);
            return Task.CompletedTask;
        }

        private Task UserLeft(SocketGuildUser userInfo)
        {
            new PlayerDataLoader().UpdatePlayerData(userInfo.Guild);
            return Task.CompletedTask;
        }

        private Task GuildMemberUpdated(
            SocketGuildUser oldUserInfo,
            SocketGuildUser newUserInfo)
        {
            new PlayerDataLoader().UpdatePlayerData(newUserInfo.Guild);
            return Task.CompletedTask;
        }

        private async Task ReactionAdded(
            Cacheable<IUserMessage, ulong> cachedMessage, 
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (!reaction.User.Value.IsBot)
            {
                await new ReceiveReactionController(reaction)
                    .RunReactionReceive();
            }
        }

        private async Task Test()
        {
            while (true)
            {
                await Task.Run(() => Thread.Sleep(2000));
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
