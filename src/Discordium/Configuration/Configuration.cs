using Newtonsoft.Json;
using System;
using System.IO;

namespace Discordium
{
    public class Configuration
    {
        [JsonIgnore]
        /// <summary> The location and name of your bot's configuration file. </summary>
        public static string FileName { get; private set; } = "config/configuration.json";
        /// <summary> Ids of users who will have owner access to the bot. </summary>
        public ulong[] Owners { get; set; }
        /// <summary> Your bot's command prefix. </summary>
        public string Prefix { get; set; } = "!";
        /// <summary> Your bot's login token. </summary>
        public string Token { get; set; } = "";
        /// <summary> Your Youtube API  token. </summary>
        public string YtToken { get; set; } = "";

        private static string syToken = "";


        public static void EnsureExists()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(file))                                 // Check if the configuration file exists.
            {
                string path = Path.GetDirectoryName(file);          // Create config directory if doesn't exist.
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var config = new Configuration();                   // Create a new configuration object.

                Console.WriteLine("Please enter your token: ");
                string token = Console.ReadLine();                  // Read the bot token from console.

                Console.WriteLine("Please enter your YT token");
                string ytoken = Console.ReadLine();

                config.Token = token;
                config.YtToken = ytoken;
                Configuration.syToken = ytoken;
                config.SaveJson();                                  // Save the new configuration object to file.
            }
            Console.WriteLine("Configuration Loaded");
        }

        /// <summary> Save the configuration to the path specified in FileName. </summary>
        public void SaveJson()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            File.WriteAllText(file, ToJson());
        }

        /// <summary> Load the configuration from the path specified in FileName. </summary>
        public static Configuration Load()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);

            Configuration cf = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(file));
            Configuration.syToken = cf.YtToken;
            return cf;
        }

        /// <summary> Convert the configuration to a json string. </summary>
        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static string getYtToken()
        {
            return syToken;
        }
    }
}
