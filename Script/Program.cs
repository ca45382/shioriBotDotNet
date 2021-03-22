using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Discord;

namespace PriconneBotConsoleApp
{
    class Program
    {
        private DiscordSocketClient client;
        //public static CommandService commands;
        //public static IServiceProvider services;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        /// <summary>
        /// ボットの起動処理
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
            var jsonSettingData = new JsonDataManager(@"./botConfig.json");

            client = new DiscordSocketClient();
            var commands = new CommandService();
            var services = new ServiceCollection().BuildServiceProvider();
            Func<SocketMessage, Task> function = CommandRecieved;
            client.MessageReceived += function;

            client.Log += Log;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            await client.LoginAsync(TokenType.Bot, jsonSettingData.m_Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        /// <summary>
        /// メッセージの受信処理
        /// </summary>
        /// <param name="msgParam"></param>
        /// <returns></returns>
        private async Task CommandRecieved(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            Console.WriteLine("{0} {1}:{2}", message.Channel.Name, message.Author.Username, message);

            if (message == null) { return; }
            // コメントがユーザーかBotかの判定
            if (message.Author.IsBot) { return; }

            await message.Channel.SendMessageAsync(message.Content.ToString());
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
