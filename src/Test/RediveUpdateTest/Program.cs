using System;
using System.Threading.Tasks;
using ShioriBot.Script;

namespace RediveUpdateTest // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            await BotInitialize.UpdateRediveDatabase();
        }
    }
}