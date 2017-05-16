using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Discordium.Models;
using System.IO;
using Discordium.Services;

namespace Discordium.Module
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {

        private static Queue<Song> queue = new Queue<Song>();
        private static IAudioClient audioclient = null;
        private readonly AudioService _service;

        public AudioModule (AudioService service)
        {
            _service = service;
        }

        [Command("play", RunMode = RunMode.Async), Summary("Joins voice chat")]
        public async Task JoinChannel(string uri ,IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.Message.Author as IGuildUser)?.VoiceChannel;
            await _service.AddSong(Context.Guild,channel,uri);
        }
    }
}
