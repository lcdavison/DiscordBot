using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace DiscordBot.Handlers
{
    class CommandHandler
    {
        private const char COMMAND_PREFIX = '!';

        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandService _commandService;

        public CommandHandler(IServiceProvider serviceProvider, DiscordSocketClient client, CommandService commandService)
        {
            _serviceProvider = serviceProvider;
            _client = client;
            _commandService = commandService;
        }

        public async Task InstallCommandHandler()
        {
            _client.MessageReceived += HandleUserCommand;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private async Task HandleUserCommand(SocketMessage socketMessage)
        {
            if (socketMessage is SocketUserMessage userMessage)
            {
                int argumentPosition = 0;

                bool hasCommandPrefix = userMessage.HasCharPrefix(COMMAND_PREFIX, ref argumentPosition);

                if (hasCommandPrefix)
                {
                    var commandContext = new SocketCommandContext(_client, userMessage);

                    await _commandService.ExecuteAsync(commandContext, argumentPosition, _serviceProvider);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }
}
