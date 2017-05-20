using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Discordium.Services;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using System;

namespace Discordium
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandHandler _commands;

        public async Task StartAsync()
        {
            Configuration.EnsureExists();                    // Ensure the configuration file has been created.
                                                             // Create a new instance of DiscordSocketClient.
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
            #if DEBUG
                LogLevel = LogSeverity.Debug,
            #else
                LogLevel = LogSeverity.Verbose,
            #endif                 
            });

            var serviceProvider = ConfigureServices();

            _client.Log += (l)                               // Register the console log event.
                => Console.Out.WriteLineAsync(l.ToString());
         
            await _client.LoginAsync(TokenType.Bot, Configuration.Load().Token);
            await _client.StartAsync();

            _commands = new CommandHandler(serviceProvider);                // Initialize the command handler service
            await _commands.ConfigureAsync();

            await Task.Delay(-1);                            // Prevent the console window from closing.
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false,ThrowOnError = false}))
                .AddSingleton<AudioService>();

            var provider = services.BuildServiceProvider();
            provider.GetService<AudioService>();

            return provider;
        }

    }


    


}