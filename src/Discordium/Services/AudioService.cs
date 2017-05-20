﻿using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Discordium.Models;
using Discord.Commands;
using Discord.Audio;
using System.Text;
using System.IO;
using Discord;
using System;

namespace Discordium.Services
{
    public class AudioService
    {

        private readonly ConcurrentDictionary<ulong, GuildVoiceContext> _guildVoiceContext = new ConcurrentDictionary<ulong, GuildVoiceContext>();

        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;

        public AudioService(CommandService commands,IServiceProvider provider)
        {
            _commands = commands;
            _provider = provider;
        }

        public async Task<string> AddSong(IGuild guild,IVoiceChannel target, string uri)
        {
            string fname = HashFileName(uri);
            bool gotTheFile = await getSongFromYoutubes(uri, fname);

            if (gotTheFile)
            {
                GuildVoiceContext audiocon;
               if(!_guildVoiceContext.TryGetValue(guild.Id,out audiocon))
                {
                    audiocon = new GuildVoiceContext();
                    audiocon.queue = new Queue<Song>();

                    if(!_guildVoiceContext.TryAdd(guild.Id,audiocon))
                    {
                        return null;
                    }

                }
                audiocon.queue.Enqueue(new Song(fname));
                JoinAudio(guild,target,audiocon);

                return fname; 

            }

            return null;
        }

        public async Task JoinAudio(IGuild guid, IVoiceChannel target, GuildVoiceContext gvc)
        {
            if(gvc.client != null )
            {        
                return;
            }

            if(target.Id == guid.Id)
            {
                return;
            }
            gvc.client = await target.ConnectAsync();

                Console.WriteLine("Connected voice on: " + guid.Name);

                    if (gvc.queue.Count > 0)
                    {
                        Song song = gvc.NextSong();
                        string filnename = "audio\\" + song.filename + ".m4a";
                        await SendAudioAsync(guid, null, filnename,gvc);
                    }        
        }

        public async Task LeaveAudio(IGuild guild, GuildVoiceContext gvc = null)
        {
            if(gvc == null)
                if (!_guildVoiceContext.TryGetValue(guild.Id, out gvc))
                {
                    Console.Write("ERRORS LEAVING");
                    return;
                }

            await gvc.client.StopAsync();
            gvc.client = null;

        }

        public void  Skip(IGuild guild)
        {
            GuildVoiceContext gvc;
            if(_guildVoiceContext.TryGetValue(guild.Id,out gvc))
            {
                gvc.player.Kill();
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

        private async Task SendAudioAsync(IGuild guild, IMessageChannel channel,string path,GuildVoiceContext gvc)
        {

                gvc.player = CreateStreamFFMPEG(path);
                var ffmpeg = gvc.player;
                var output = ffmpeg.StandardOutput.BaseStream;
                var discord = gvc.client.CreatePCMStream(AudioApplication.Mixed, 96000);
                await output.CopyToAsync(discord);
                await discord.FlushAsync();

                if(gvc.queue.Count > 0)
                {
                    Song song = gvc.NextSong();
                    string filnename = "audio\\" + song.filename + ".m4a";
                    await SendAudioAsync(guild, channel, filnename,gvc);  
                 }          
                if (gvc.client.ConnectionState == ConnectionState.Connected && gvc.queue.Count == 0)
                    await LeaveAudio(guild);
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

        public string getCurrentlyPlayingSong(IGuild guild)
        {
            GuildVoiceContext context;

            if (_guildVoiceContext.TryGetValue(guild.Id, out context))
            {
                if (context.player != null)
                    return context.playing.filename;
            }
            return null;
        }

        public List<string> getQueue(IGuild guild)
        {
            GuildVoiceContext context;

            if (_guildVoiceContext.TryGetValue(guild.Id, out context))
            {
                if (context.queue.Count > 0)
                {
                    List<string> songnames = new List<string>();

                    foreach (Song s in context.queue)
                    {
                        songnames.Add(s.filename);
                    }

                    return songnames;
                }            
            }
            return null;
        }

        public string getLastSong(IGuild guild)
        {
            GuildVoiceContext context;

            if (_guildVoiceContext.TryGetValue(guild.Id, out context))
            {
                if (context.player != null)
                    return context.lastSong.filename;
            }
            return null;
        }
    }
}