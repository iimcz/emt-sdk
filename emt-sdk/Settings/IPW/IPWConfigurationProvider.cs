using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace emt_sdk.Settings.IPW
{
    public class IPWConfigurationProvider : IConfigurationProvider<IPWSetting>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static string ConfigDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "emt");
        private static string ConfigPath => Path.Combine(ConfigDirectory, "ipw.json");

        public IPWSetting Configuration
        { 
            get
            {
                if (_config == null)
                {
                    // Create default config if it doesn't exist already
                    _config = LoadConfiguration() ?? new IPWSetting
                    {
                        Displays = new List<DisplaySetting>
                        {
                            new DisplaySetting(),
                            new DisplaySetting
                            {
                                DisplayId = 1
                            }
                        }
                    };

                    SaveConfiguration();
                }

                return _config;
            }
        }

        public bool ConfigurationExists => File.Exists(ConfigPath);

        private IPWSetting _config;

        public void SaveConfiguration()
        {
            Directory.CreateDirectory(ConfigDirectory);

            // Keep a backup of old file
            if (ConfigurationExists)
            {
                var fileNameFriendlyDate = DateTime.Now.ToString("s").Replace(":", "");
                var backupPath = Path.Combine(ConfigDirectory, $"ipw_{fileNameFriendlyDate}.json");
                File.Move(ConfigPath, backupPath);
            }

            var json = JsonConvert.SerializeObject(_config);
            File.WriteAllText(ConfigPath, json);
        }

        private IPWSetting LoadConfiguration()
        {
            if (!ConfigurationExists) return null;

            try
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonConvert.DeserializeObject<IPWSetting>(json);
            }
            catch (JsonException ex)
            {
                Logger.Error(ex, "Failed to deserialize IPW configuration file");
                return null;
            }
        }
    }
}
