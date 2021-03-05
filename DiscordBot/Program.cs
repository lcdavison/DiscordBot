using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commandService;
        private CommandHandler _commandHandler;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            CreateDiscordClient();

            await LoadTokenAndLogin();

            SetupEventCallbacks();

            await CreateCommandServiceAndHandler();

            await Task.Delay(-1);
        }

        private void CreateDiscordClient()
        {
            var socketClientConfig = new DiscordSocketConfig
            {
                MessageCacheSize = 100
            };

            _client = new DiscordSocketClient(socketClientConfig);
        }

        private async Task LoadTokenAndLogin()
        {
            string token;

            using (var streamReader = new StreamReader("token.txt"))
            {
                token = streamReader.ReadLine();
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }

        private void SetupEventCallbacks()
        {
            _client.Ready += () =>
            {
                Console.WriteLine("Discord bot is ready!");
                return Task.CompletedTask;
            };
        }

        private async Task CreateCommandServiceAndHandler()
        {
            _commandService = new CommandService();

            _commandHandler = new CommandHandler(_client, _commandService);
            await _commandHandler.InstallCommandHandler();
        }
    }
}
