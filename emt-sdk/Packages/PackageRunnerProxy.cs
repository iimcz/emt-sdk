using System;

namespace emt_sdk.Packages
{
    public class PackageRunnerProxy : IPackageRunner
    {
        public Action<PackageDescriptor> PackageRunAction { get; set; }

        public void RunPackage(PackageDescriptor package)
        {
            if (PackageRunAction == null)
            {
                throw new InvalidOperationException($"Cannot run a package before a run action is set! {this.GetHashCode()}");
            }
            PackageRunAction(package);
        }
    }
}