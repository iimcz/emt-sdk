using System;

namespace emt_sdk.Packages
{
    public class PackageRunnerProxy : IPackageRunner
    {
        public Action<PackageDescriptor> PackageRunAction { get; set; }

        public void RunPackage(PackageDescriptor package)
        {
            PackageRunAction(package);
        }
    }
}