using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using ShioriBot.Define;
using ShioriBot.Model;
using Discord.Commands;
using System.Reflection;

namespace ShioriBot.Script
{
    public class Handler
    {
        private readonly DiscordSocketClient m_DiscordSocketClient;
        private readonly CommandService m_CommandService;
        private readonly IServiceProvider m_ServiceProvider;


        public Handler(DiscordSocketClient client, CommandService commandService, IServiceProvider serviceProvider) 
        {
            m_DiscordSocketClient = client;
            m_CommandService = commandService;
            m_ServiceProvider = serviceProvider;
        }

        public async Task InitializeAsync()
        {
            m_DiscordSocketClient.MessageReceived += CommandRecieved;
            m_DiscordSocketClient.Ready += Client_Ready;
            m_DiscordSocketClient.GuildMemberUpdated += GuildMemberUpdated;
            m_DiscordSocketClient.InteractionCreated += InteractionCreated;
            m_DiscordSocketClient.Log += Log;

            await m_CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), m_ServiceProvider);

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
        private async Task Client_Ready()
        {
            var guildList = m_DiscordSocketClient.Guilds;

            await Task.Run(() =>
            {
                foreach (var guild in guildList)
                {
                    var discordDataLoader = new DiscordDataLoader();
                    discordDataLoader.UpdateServerData(guild);
                    discordDataLoader.UpdateClanData(guild);
                    discordDataLoader.UpdatePlayerData(guild);
                }
            });
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
            await socketInteraction.DeferAsync();
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
