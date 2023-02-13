using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Naki3D.Common.Protocol;
using System.Threading.Tasks;

namespace emt_sdk.ScenePackage
{
    public class PackageService : Naki3D.Common.Protocol.PackageService.PackageServiceBase
    {
        public override Task<BoolValue> LoadPackage(LoadPackageRequest request, ServerCallContext context)
        {
            return base.LoadPackage(request, context);
        }

        public override Task<BoolValue> ClearStartupPackage(ClearStartupPackageRequest request, ServerCallContext context)
        {
            return base.ClearStartupPackage(request, context);
        }

        public override Task<StartupPackageResponse> GetStartupPackage(Empty request, ServerCallContext context)
        {
            return base.GetStartupPackage(request, context);
        }

        public override Task<BoolValue> SetStartupPackage(SetStartupPackageRequest request, ServerCallContext context)
        {
            return base.SetStartupPackage(request, context);
        }

        public override Task<Empty> PurgeCachedPackages(Empty request, ServerCallContext context)
        {
            return base.PurgeCachedPackages(request, context);
        }

        public override Task<CachedPackagesResponse> GetCachedPackages(Empty request, ServerCallContext context)
        {
            return base.GetCachedPackages(request, context);
        }

        public override Task<BoolValue> StartPackage(StartPackageRequest request, ServerCallContext context)
        {
            return base.StartPackage(request, context);
        }
    }
}
