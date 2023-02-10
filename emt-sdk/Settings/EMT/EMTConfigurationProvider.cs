using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

namespace emt_sdk.Settings.EMT
{
    public class EMTConfigurationProvider : IConfigurationProvider<EMTSetting>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static string ConfigDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "emt");
        private static string ConfigPath => Path.Combine(ConfigDirectory, "emt.json");

        public EMTSetting Configuration
        {
            get
            {
                if (_config == null)
                {
                    // Create default config if it doesn't exist already
                    _config = LoadConfiguration() ?? new EMTSetting();
                    SaveConfiguration();
                }

                return _config;
            }
        }

        public bool ConfigurationExists => File.Exists(ConfigPath);

        private EMTSetting _config;

        public void SaveConfiguration()
        {
            Directory.CreateDirectory(ConfigDirectory);

            var json = JsonConvert.SerializeObject(_config);
            File.WriteAllText(ConfigPath, json);
        }

        private EMTSetting LoadConfiguration()
        {
            if (!ConfigurationExists) return null;

            try
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonConvert.DeserializeObject<EMTSetting>(json);
            }
            catch (JsonException ex)
            {
                Logger.Error(ex, "Failed to deserialize EMT configuration file");
                return null;
            }
        }
    }
}
