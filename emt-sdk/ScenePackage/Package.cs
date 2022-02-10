using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using emt_sdk.ScenePackage;

namespace emt_sdk.Generated.ScenePackage
{
    /// <summary>
    /// Implementation of <see cref="PackageDescriptor"/> logic.
    /// </summary>
    public partial class PackageDescriptor
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        public static readonly string DefaultPackageStore;
        public static readonly string PackageStore;

        static PackageDescriptor()
        {
            DefaultPackageStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "emt");
            PackageStore = Environment.GetEnvironmentVariable("EMT_PATH") ?? DefaultPackageStore;
            
            Directory.CreateDirectory(PackageStore);
        }
        
        /// <summary>
        /// Downloads a file specified by package.
        /// </summary>
        public void DownloadFile()
        {
            using (var client = new WebClient()) client.DownloadFile(Package.Url, ArchivePath);

            if (!VerifyChecksum(Package.Checksum))
            {
                Logger.Error($"Checksum verification failed for package '{ArchiveFileName}', aborting");
                File.Delete(ArchivePath);
                
                throw new InvalidDataException();
            }
            
            // Directory for extracted zip
            Directory.CreateDirectory(PackageDirectory);
            
            using (var stream = new FileStream(ArchivePath, FileMode.Open))
            using (var zip = new ZipArchive(stream))
            {
                zip.ExtractToDirectory(PackageDirectory);
            }
        }

        public bool VerifyChecksum(string checksum)
        {
            using (var hasher = HashAlgorithm.Create("SHA256"))
            using (FileStream stream = new FileStream(ArchivePath, FileMode.Open))
            {
                StringBuilder sb = new StringBuilder();
                var bytes = hasher.ComputeHash(stream);
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                if (sb.ToString() != checksum)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the archive filename of this package
        /// </summary>
        /// <returns>Package archive filename</returns>
        public string ArchiveFileName
        {
            get {
                var url = Package.Url;
                var queryDictionary = System.Web.HttpUtility.ParseQueryString(url.Query);
                return queryDictionary["packageName"];
            }
        }

        public string ArchivePath => Path.Combine(PackageStore, ArchiveFileName);

        /// <summary>
        /// Gets the location of the extracted package
        /// </summary>
        public string PackageDirectory => Path.Combine(PackageStore, Path.GetFileNameWithoutExtension(ArchivePath));

        /// <summary>
        /// Gets the location of package resources (video, models, images, etc...)
        /// </summary>
        public string DataRoot => Path.Combine(PackageDirectory, "dataroot");

        /// <summary>
        /// Checks whether a package is downloaded
        /// </summary>
        public bool IsDownloaded() => File.Exists(ArchivePath);

        /// <summary>
        /// Removes package contents from local storage
        /// </summary>
        public void RemoveFile()
        {
            if (!IsDownloaded()) throw new FileNotFoundException();
            File.Delete(ArchiveFileName);
            Directory.Delete(PackageDirectory);
        }

        /// <summary>
        /// Runs program associated with package. Only used for 3D scenes.
        /// </summary>
        /// <returns>Launched process</returns>
        public Process Run()
        {
            if (Parameters.DisplayType != "scene") throw new NotSupportedException();
            return Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(DataRoot, "scene.x86_64")
            });
        }
    }
}
