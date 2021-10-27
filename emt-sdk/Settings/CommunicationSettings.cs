namespace emt_sdk.Settings
{
    public class CommunicationSettings
    {
        /// <summary>
        /// Hostname of the Content Manager server
        /// </summary>
        public string ContentHostname { get; set; }

        /// <summary>
        /// Port of the Content Manager server
        /// </summary>
        public int ContentPort { get; set; }

        /// <summary>
        /// IP used by <see cref="Events.EventManager"/> for listening for incoming sensor events
        /// </summary>
        public string SensorListenIp { get; set; }

        /// <summary>
        /// Port used by <see cref="Events.EventManager"/> for listening for incoming sensor events
        /// </summary>
        public int SensorListenPort { get; set; }
    }
}
