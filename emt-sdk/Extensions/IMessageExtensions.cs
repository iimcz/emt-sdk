using Google.Protobuf;
using System.IO;
using System.Text;

namespace emt_sdk.Extensions
{
    public static class IMessageExtensions
    {
        public static void WriteJsonTo(this IMessage message, Stream stream)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonFormatter.Default.Format(message));
            int length = data.Length;
            stream.Write(data, 0, length);
        }
    }
}
