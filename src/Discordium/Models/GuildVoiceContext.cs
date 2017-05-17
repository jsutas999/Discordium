using System.Diagnostics;
using Discord.Audio;
using Discordium.Models;
using System.Collections.Generic;

namespace Discordium.Models
{
    public class GuildVoiceContext
    {
        public IAudioClient client;
        public Queue<Song> queue = new Queue<Song>();
        public Process player;
    }
}
