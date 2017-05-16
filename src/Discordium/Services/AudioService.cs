using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System;

namespace Discordium.Services
{
    public class AudioService
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        public string nab = "WHAT THA FYCK";

        public AudioService(CommandService commands,IServiceProvider provider)
        {
            _commands = commands;
            _provider = provider;
        }

       

    }
}
