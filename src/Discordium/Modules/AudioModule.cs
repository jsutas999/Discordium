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

namespace Discordium.Modules 
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
            await ReplyAsync("FUGTHISSHIT");
            await ReplyAsync(_service.nab);

            channel = channel ?? (Context.Message.Author as IGuildUser)?.VoiceChannel;

            string fname = HashFileName(uri);
            bool gotTheFile = await getSongFromYoutubes(uri, fname);
            if ( gotTheFile )
            {
                queue.Enqueue(new Song(fname));

                if (audioclient == null)
                {
                    audioclient = await channel.ConnectAsync();
                    await SendAsync("audio\\" + queue.Dequeue().filename + ".m4a");
                }

                else if (audioclient.ConnectionState == ConnectionState.Disconnected)
                {
                    audioclient = await channel.ConnectAsync();
                    await SendAsync("audio\\" + queue.Dequeue().filename + ".m4a");
                }
             }
        }

        private Process CreateStreamFFMPEG(string path)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-loglevel panic -i {path} -ac 2 -f s16le -b:a 64k -ar 48000  pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            return Process.Start(ffmpeg);
        }

        private Process DownloadFromYoutube(string url,string filename)
        {
            var ytdl = new ProcessStartInfo
            {
                FileName = "youtube-dl",
                Arguments = $"-f 140 -o \"audio\\{filename}.%(ext)s\" {url}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            return Process.Start(ytdl);
        }

        private async Task SendAsync(string path)
        {
            var ffmpeg = CreateStreamFFMPEG(path);
            var output = ffmpeg.StandardOutput.BaseStream;
            var discord = audioclient.CreatePCMStream(AudioApplication.Mixed, 96000);
            await output.CopyToAsync(discord);
            await discord.FlushAsync();


            if (queue.Count > 0)
                await SendAsync("audio\\" +queue.Dequeue().filename + ".m4a");

            if (audioclient.ConnectionState == ConnectionState.Connected && queue.Count == 0)
                await audioclient.StopAsync();
        }

        private async Task<bool> getSongFromYoutubes(string uri,string fname)
        {
            var downloader = DownloadFromYoutube(uri, fname);
            downloader.WaitForExit();

            if (File.Exists($"audio\\{fname}.m4a"))
            {
                return true;
            }
            else return false;


        }

        private string HashFileName(string filename)
        {
            string output;
            using (MD5 hasher = MD5.Create())
            {
                byte[] data = hasher.ComputeHash(Encoding.UTF8.GetBytes(filename));

                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < data.Length;i++)
                {
                    sb.Append(data[i].ToString("x2"));
                }

                output = sb.ToString();

            }

            return output;

        }
    }
}
