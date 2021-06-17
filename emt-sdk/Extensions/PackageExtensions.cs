using emt_sdk.Generated.ScenePackage;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace emt_sdk.Extensions
{
    public static class PackageExtensions
    {
        public static void DownloadFile(this Package package, string directory)
        {
            if (!Directory.Exists(directory)) throw new DirectoryNotFoundException();

            var url = package.PackagePackage.Url;
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(url.Query);
            var filePath = Path.Combine(directory, queryDictionary["packageName"]);

            using (var client = new WebClient()) client.DownloadFile(url, filePath);

            using (var hasher = HashAlgorithm.Create("SHA256"))
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                StringBuilder sb = new StringBuilder();
                var bytes = hasher.ComputeHash(stream);
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                if (sb.ToString() != package.PackagePackage.Checksum) throw new InvalidDataException();
            }
        }
    }
}
