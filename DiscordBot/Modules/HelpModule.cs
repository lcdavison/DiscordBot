using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpCommand()
        {
            await Context.Channel.SendMessageAsync("Here is a list of available commands: ");
        }
    }
}
