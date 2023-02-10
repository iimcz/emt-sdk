using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

namespace emt_sdk.Settings.PGE
{
    public class PGEConfigurationProvider : IConfigurationProvider<PGESetting>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static string ConfigDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "emt");
        private static string ConfigPath => Path.Combine(ConfigDirectory, "pge.json");

        public PGESetting Configuration
        { 
            get
            {
                if (_config == null)
                {
                    // Create default config if it doesn't exist already
                    _config = LoadConfiguration() ?? new PGESetting();
                    SaveConfiguration();
                }

                return _config;
            }
        }

        public bool ConfigurationExists => File.Exists(ConfigPath);

        private PGESetting _config;

        public void SaveConfiguration()
        {
            Directory.CreateDirectory(ConfigDirectory);

            // Keep a backup of old file
            if (ConfigurationExists)
            {
                var fileNameFriendlyDate = DateTime.Now.ToString("s").Replace(":", "");
                var backupPath = Path.Combine(ConfigDirectory, $"pge_{fileNameFriendlyDate}.json");
                File.Move(ConfigPath, backupPath);
            }

            var json = JsonConvert.SerializeObject(_config);
            File.WriteAllText(ConfigPath, json);
        }

        private PGESetting LoadConfiguration()
        {
            if (!ConfigurationExists) return null;

            try
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonConvert.DeserializeObject<PGESetting>(json);
            }
            catch (JsonException ex)
            {
                Logger.Error(ex, "Failed to deserialize PGE configuration file");
                return null;
            }
        }
    }
}
