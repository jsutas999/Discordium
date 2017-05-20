using System.Collections.Generic;
using System.Diagnostics;
using Discordium.Models;
using Discord.Audio;

namespace Discordium.Models
{
    public class GuildVoiceContext
    {
        public IAudioClient client;
        public Queue<Song> queue = new Queue<Song>();
        public Process player;
        public Song playing;
        public Song lastSong;

        public Song NextSong()
        {
            Song song = queue.Dequeue();
            lastSong = playing;
            playing = song;
            return song;
        }
    }
}
