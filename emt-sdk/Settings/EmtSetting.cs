using System.IO;
using Naki3D.Common.Protocol;
using Newtonsoft.Json;
using Environment = System.Environment;

namespace emt_sdk.Settings
{
    /// <summary>
    /// Settings for the entire EMT device
    /// </summary>
    public class EmtSetting
    {
        private static string ConfigDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "emt");
        private static string ConfigPath => Path.Combine(ConfigDirectory, "emt.json");
        
        /// <summary>
        /// Type of current device
        /// </summary>
        public DeviceType Type { get; set; }
        
        /// <summary>
        /// Performance capabilities of current device
        /// </summary>
        public PerformanceCap PerformanceCap { get; set; }
        
        /// <summary>
        /// Settings for all remote connections
        /// </summary>
        public CommunicationSettings Communication { get; set; } = new CommunicationSettings();

        /// <summary>
        /// Name of default package loaded at startup
        /// </summary>
        public string StartupPackage { get; set; }

        public void Save()
        {
            Directory.CreateDirectory(ConfigDirectory);
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(ConfigPath, json);
        }

        /// <summary>
        /// Attempts to load the config from the default location, otherwise returns <see langword="null"/>
        /// </summary>
        public static EmtSetting FromConfig()
        {
            if (!File.Exists(ConfigPath)) return null;

            var json = File.ReadAllText(ConfigPath);
            return JsonConvert.DeserializeObject<EmtSetting>(json);
        }
    }
}