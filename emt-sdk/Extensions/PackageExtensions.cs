﻿using emt_sdk.Generated.ScenePackage;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace emt_sdk.Extensions
{
    /// <summary>
    /// Implementation of <see cref="Package"/> logic. Extensions are used due to <see cref="Package"/> being autogenerated.
    /// </summary>
    public static class PackageExtensions
    {
        /// <summary>
        /// Downloads a file specified by package.
        /// </summary>
        /// <param name="package">Package contatining file info</param>
        /// <param name="directory">Target download directory</param>
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

        /// <summary>
        /// Gets the filename of a package
        /// </summary>
        /// <param name="package">Queried package</param>
        /// <returns>Package filename</returns>
        public static string FileName(this Package package)
        {
            var url = package.PackagePackage.Url;
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(url.Query);
            return queryDictionary["packageName"];
        }

        /// <summary>
        /// Checks whether a package is downloaded
        /// </summary>
        /// <param name="package">Package to be checked</param>
        /// <param name="directory">Download directory</param>
        /// <returns>Whether the package is downloaded</returns>
        public static bool IsDownloaded(this Package package, string directory)
        {
            var filePath = Path.Combine(directory, package.FileName());
            return File.Exists(filePath);
        }

        /// <summary>
        /// Removes package contents from local storage
        /// </summary>
        /// <param name="package">Package to be removed</param>
        /// <param name="directory">Download directory</param>
        public static void RemoveFile(this Package package, string directory)
        {
            if (!IsDownloaded(package, directory)) throw new FileNotFoundException();
            File.Delete(package.FileName());
        }

        /// <summary>
        /// Runs program associated with package. Only used for 3D scenes.
        /// </summary>
        /// <param name="package">Executed package</param>
        /// <param name="directory">Download directory</param>
        /// <returns>Launched process</returns>
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
