namespace emt_sdk.Diagnostics
{
    /// <summary>
    /// Provides diagnostic logs depending on runtime and system
    /// </summary>
    public interface IDiagnosticLogProvider
    {
        /// <summary>
        /// Reads the entire contents of the main log file depending on runtime
        /// </summary>
        /// <returns>Log contents</returns>
        string[] ReadMainLog();

        /// <summary>
        /// Reads a specific tagged log file depending on implementation
        /// </summary>
        /// <param name="tag">Target log</param>
        /// <returns>Log contents</returns>
        string[] ReadTaggedLog(string tag);
    }
}
