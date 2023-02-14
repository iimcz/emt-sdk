using emt_sdk.Settings;
using emt_sdk.Settings.EMT;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Naki3D.Common.Protocol;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace emt_sdk.Packages
{
    public class PackageService : Naki3D.Common.Protocol.PackageService.PackageServiceBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly PackageLoader _packageLoader;
        private readonly IConfigurationProvider<EMTSetting> _config;
        private readonly IPackageRunner _runner;

        public PackageService(PackageLoader packageLoader, IConfigurationProvider<EMTSetting> config, IPackageRunner runner)
        {
            _packageLoader = packageLoader;
            _config = config;
            _runner = runner;
        }

        public override Task<BoolValue> LoadPackage(LoadPackageRequest request, ServerCallContext context)
        {
            // TODO: Preview mode
            var sr = new StringReader(request.DescriptorJson);
            var package = _packageLoader.LoadPackage(sr, false);

            try
            {
                package.DownloadFile();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load package");
                return Task.FromResult(new BoolValue { Value = false });
            }

            return Task.FromResult(new BoolValue { Value = true });
        }

        public override Task<BoolValue> ClearStartupPackage(ClearStartupPackageRequest request, ServerCallContext context)
        {
            if (request.PurgeData)
            {
                var startupPackage = _packageLoader.FindPackage(_config.Configuration.StartupPackage, false);
                if (startupPackage == null)
                {
                    Logger.Warn($"Cannot purge package '{_config.Configuration.StartupPackage}' because it was not found, skipping");
                }
                else
                {
                    startupPackage.RemoveFile();
                }
            }

            _config.Configuration.StartupPackage = "";
            _config.SaveConfiguration();

            return Task.FromResult(new BoolValue { Value = true });
        }

        public override Task<StartupPackageResponse> GetStartupPackage(Empty request, ServerCallContext context)
        {
            var response = new StartupPackageResponse
            {
                PackageId = _config.Configuration.StartupPackage
            };

            return Task.FromResult(response);
        }

        public override Task<BoolValue> SetStartupPackage(SetStartupPackageRequest request, ServerCallContext context)
        {
            var package = _packageLoader.FindPackage(request.PackageId, false);
            if (package == null)
            {
                Logger.Error($"Cannot set startup package to '{request.PackageId}' because it is not downloaded");
                return Task.FromResult(new BoolValue { Value = false });
            }

            _config.Configuration.StartupPackage = request.PackageId;
            _config.SaveConfiguration();

            return Task.FromResult(new BoolValue { Value = true });

        }

        public override Task<Empty> PurgeCachedPackages(Empty request, ServerCallContext context)
        {
            var packages = _packageLoader.EnumeratePackages(false);
            foreach (var package in packages) package.RemoveFile();

            Logger.Info("Purged all cached packages");

            return Task.FromResult(new Empty());
        }

        public override Task<CachedPackagesResponse> GetCachedPackages(Empty request, ServerCallContext context)
        {
            var packages = _packageLoader.EnumeratePackages(false)
                .Select(p => new CachedPackagesResponse.Types.SinglePackage
                {
                    Checksum = p.Package.Checksum,
                    DownloadTime = File.GetCreationTime(p.ArchiveFileName).ToTimestamp(),
                    PackageId = p.Metadata.Id
                });

            var response = new CachedPackagesResponse();
            response.Packages.AddRange(packages);
            return Task.FromResult(response);
        }

        public override Task<BoolValue> StartPackage(StartPackageRequest request, ServerCallContext context)
        {
            var package = _packageLoader.FindPackage(request.PackageId, false);
            if (package == null)
            {
                Logger.Error($"Could not find package '{request.PackageId}', cannot start");
                return Task.FromResult(new BoolValue { Value = false });
            }

            _runner.RunPackage(package);
            return Task.FromResult(new BoolValue { Value = true });
        }
    }
}
