using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

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
            if (IsDownloaded())
            {
                Logger.Info($"Package '{Package.Checksum}' already downloaded, skipping");
                return;
            }

            Logger.Info($"Downloading package '{Metadata.PackageName}' / '{Metadata.Id}' from '{Package.Url}'");
            using (var client = new WebClient()) client.DownloadFile(Package.Url, ArchivePath);
            Logger.Info($"Download complete");

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

            Logger.Info($"Extracted package to '{ArchivePath}'");
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
            get
            {
                return Package.Checksum + ".zip";
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
            File.Delete(ArchivePath);
            Directory.Delete(PackageDirectory, true);

            Logger.Info($"Removed package '{ArchivePath}'");
        }

        /// <summary>
        /// Runs program associated with package. Only used for 3D scenes.
        /// </summary>
        /// <returns>Launched process</returns>
        public Process Run()
        {
            if (Parameters.DisplayType != "scene")
            {
                Logger.Error($"Attempted to launch a non-native package '{ArchivePath}' as native");
                throw new NotSupportedException($"Attempted to launch a non-native package '{ArchivePath}' as native");
            }

            var fileName = Path.Combine(DataRoot, Parameters?.Settings?.FileName ?? "scene.x86_64");
            
            // We need to set the execute bit on Linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x '{fileName}'"
                }).WaitForExit();
            }

            Logger.Info($"Launching native 3D scene package '{ArchivePath}'");
            return Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false
            });
        }
    }
}
