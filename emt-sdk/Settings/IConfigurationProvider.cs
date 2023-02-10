namespace emt_sdk.Settings
{
    /// <summary>
    /// Provides a way to load and save EMT related configurations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConfigurationProvider<T> where T : class
    {
        /// <summary>
        /// A shared configuration instance.
        /// </summary>
        T Configuration { get; }

        /// <summary>
        /// Whether or not a valid configuration exists.
        /// </summary>
        bool ConfigurationExists { get; }

        /// <summary>
        /// Saves the shared configuration to the default location.
        /// </summary>
        /// <param name="config">Configuration to be saved</param>
        void SaveConfiguration();
    }
}
