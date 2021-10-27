using Google.Protobuf;
using System.IO;
using System.Text;

namespace emt_sdk.Extensions
{
    /// <summary>
    /// Extensions for protobug communication
    /// </summary>
    public static class IMessageExtensions
    {
        /// <summary>
        /// Sends a message as JSON into a stream
        /// </summary>
        /// <param name="message">Protobuf message</param>
        /// <param name="stream">Target stream</param>
        public static void WriteJsonTo(this IMessage message, Stream stream)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonFormatter.Default.Format(message));
            int length = data.Length;
            stream.Write(data, 0, length);
        }
    }
}
