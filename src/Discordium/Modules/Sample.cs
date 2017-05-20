using System.Threading.Tasks;
using Discord.Commands;


namespace Discordium.Modules
{
    [Name("Example")]
    public class Sample : ModuleBase<SocketCommandContext>
    {

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
