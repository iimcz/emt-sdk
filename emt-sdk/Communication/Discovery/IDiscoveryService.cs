namespace emt_sdk.Communication.Discovery
{
    public interface IDiscoveryService
    {
        /// <summary>
        /// Whether the service is sending out beacon messages
        /// </summary>
        bool IsBroadcasting { get; }

        /// <summary>
        /// Starts periodically broadcasting beacon messages
        /// </summary>
        void StartBroadcast();

        /// <summary>
        /// Stops broadcasting beacon messages
        /// </summary>
        void StopBroadcast();
    }
}
