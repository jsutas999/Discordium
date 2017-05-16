using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using Discordium.Models;
using System.Text;
using System.Security.Cryptography;

namespace Discordium.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<Song>> SongQueues = new ConcurrentDictionary<ulong, ConcurrentQueue<Song>>();

        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;

        public AudioService(CommandService commands,IServiceProvider provider)
        {
            _commands = commands;
            _provider = provider;
        }


        public async Task<int> AddSong(IGuild guild,IVoiceChannel target, string uri)
        {

            string fname = HashFileName(uri);
            bool gotTheFile = await getSongFromYoutubes(uri, fname);

            if (gotTheFile)
            {

                ConcurrentQueue<Song> queue;
                if(!SongQueues.TryGetValue(guild.Id,out queue))
                {
                    queue = new ConcurrentQueue<Song>();
                    if(!SongQueues.TryAdd(guild.Id,queue))
                    {
                        return -1;
                    }
                }

                queue.Enqueue(new Song(fname));
                await JoinAudio(guild,target);

            }

            return 0;
        }

        public async Task JoinAudio(IGuild guid, IVoiceChannel target)
        {
            IAudioClient client;

            if(ConnectedChannels.TryGetValue(guid.Id, out client))
            {        
                return;
            }
            if(target.Id == guid.Id)
            {
                return;
            }
            var audioClient = await target.ConnectAsync();

            if(ConnectedChannels.TryAdd(guid.Id,audioClient))
            {
                Console.WriteLine("Connected voice on: " + guid.Name);

                ConcurrentQueue<Song> queue;
                if (SongQueues.TryGetValue(guid.Id, out queue))
                {
                    if (queue.Count > 0)
                    {
                        Song song;
                        if (queue.TryDequeue(out song))
                        {
                            string filnename = "audio\\" + song.filename + ".m4a";
                            await SendAudioAsync(guid, null, filnename);
                        }
                    }
                }

            }
        }

        public async Task LeaveAudio(IGuild guid)
        {
            IAudioClient client;
            if(ConnectedChannels.TryRemove (guid.Id, out client) )
            {
                await client.StopAsync();
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

        private Process DownloadFromYoutube(string url, string filename)
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

        private async Task SendAudioAsync(IGuild guild, IMessageChannel channel,string path)
        {

            IAudioClient audioclient;

            if(ConnectedChannels.TryGetValue(guild.Id,out audioclient))
            {
                var ffmpeg = CreateStreamFFMPEG(path);
                var output = ffmpeg.StandardOutput.BaseStream;
                var discord = audioclient.CreatePCMStream(AudioApplication.Mixed, 96000);
                await output.CopyToAsync(discord);
                await discord.FlushAsync();

                ConcurrentQueue<Song> queue;
                if (SongQueues.TryGetValue(guild.Id, out queue) )
                {
                    if (queue.Count > 0)
                    {
                        Song song;
                        if(queue.TryDequeue(out song))
                        {
                            string filnename = "audio\\" + song.filename + ".m4a";
                            await SendAudioAsync(guild, channel, filnename);
                        }
                    }
                }
                if (audioclient.ConnectionState == ConnectionState.Connected && queue.Count == 0)
                    await LeaveAudio(guild);
            }
        }

        private async Task<bool> getSongFromYoutubes(string uri, string fname)
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
                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("x2"));
                }

                output = sb.ToString();

            }

            return output;

        }
    }
}
