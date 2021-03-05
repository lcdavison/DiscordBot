using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot
{
    class CommandHandler
    {
        private const char COMMAND_PREFIX = '!';

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;

        public CommandHandler(DiscordSocketClient client, CommandService commandService)
        {
            _client = client;
            _commandService = commandService;
        }

        public async Task InstallCommandHandler()
        {
            _client.MessageReceived += HandleUserCommand;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
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

                    await _commandService.ExecuteAsync(commandContext, argumentPosition, null);
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
