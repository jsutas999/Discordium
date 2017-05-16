using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Discordium.Modules
{
    [Name("Example")]
    public class Sample : ModuleBase<SocketCommandContext>
    {

        String mem = "MEMES I NEED MEMS";

        [Command("say"), Summary("Echo of the wind")]
        public async Task Say([Remainder] string echo)
        {
            await ReplyAsync(echo);
        }

        [Command("math"), Summary("Adds stuff")]
        public async Task Math(int a, int b)
        {
            await ReplyAsync((a + b).ToString());
        }

    }
}
