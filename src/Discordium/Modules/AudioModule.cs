using System.Collections.Generic;
using System.Threading.Tasks;
using Discordium.Services;
using Discordium.Models;
using Discord.Commands;
using Discord.Audio;
using Discord;

namespace Discordium.Module
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {

        private static Queue<Song> queue = new Queue<Song>();
        private readonly AudioService _service;

        public AudioModule (AudioService service)
        {
            _service = service;
        }

        [Command("play", RunMode = RunMode.Async), Summary("Joins voice chat")]
        public async Task JoinChannel(string uri ,IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.Message.Author as IGuildUser)?.VoiceChannel;
            string song  = await _service.AddSong(Context.Guild,channel,uri);

            if (song != null)
                await ReplyAsync("Added song:  " + "**" + song + "**");
            else
                await ReplyAsync("Something went wrong");
        }

        [Command("skip")]
        public async Task SkipSong()
        {
            await ReplyAsync("God bless memes");
            _service.Skip(Context.Guild);
        }

        [Command("playing")]
        public  async Task Playing()
        {
            string song = _service.getCurrentlyPlayingSong(Context.Guild);

            if (song != null)
            {
                await ReplyAsync(" :white_check_mark: Currently playing: " + "**"+ song + "**");
            }
            else
                await ReplyAsync(" :no_entry_sign: There is no song playing");
        }

        [Command("queue")]
        public async Task SongQueue()
        {
            List<string> songs = _service.getQueue(Context.Guild);              
            if (songs != null)
            {
                string reply = "Songs in the queue: \n";
                foreach (string s in songs)
                {
                    reply += s;
                    reply += " \n";
                }
                await ReplyAsync(reply);
            }
            else
                await ReplyAsync(" :no_entry_sign:  There are no songs in the queue");
        }

        [Command("lastsong")]
        public async Task LastSont()
        {
            string song = _service.getLastSong(Context.Guild);

            if (song != null)
                await ReplyAsync("Last song played was: " + "**" + song + "**");
            else
                await ReplyAsync(":no_entry_sign: I havent played any songs in a while");
        }
    }
}
