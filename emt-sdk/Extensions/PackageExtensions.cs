using emt_sdk.Generated.ScenePackage;
using System.Diagnostics;
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
            var filePath = package.FileName();

            using (var client = new WebClient()) client.DownloadFile(package.PackagePackage.Url, filePath);

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

        public static string FileName(this Package package)
        {
            var url = package.PackagePackage.Url;
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(url.Query);
            return queryDictionary["packageName"];
        }

        public static bool IsDownloaded(this Package package, string directory)
        {
            var filePath = Path.Combine(directory, package.FileName());
            return File.Exists(filePath);
        }

        public static void RemoveFile(this Package package, string directory)
        {
            if (!IsDownloaded(package, directory)) throw new FileNotFoundException();
            File.Delete(package.FileName());
        }

        public static Process Run(this Package package, string directory)
        {
            var filePath = Path.Combine(directory, package.FileName());
            return Process.Start(new ProcessStartInfo
            {
                FileName = filePath
            });
        }
    }
}
