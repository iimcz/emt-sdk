using Naki3D.Common.Protocol;
using System;
namespace emt_sdk.Extensions
{
    public static class VersionExtensions
    {
        public static VersionInfo ToVersionInfo(this Version version)
        {
            // MS defines their versioning as major.minor[.build[.revision]], so the swap of build and patch are intentional
            return new VersionInfo
            {
                Build = version.Revision.ToString(),
                Patch = (uint)version.Build,
                Minor = (uint)version.Minor,
                Major = (uint)version.Major
            };
        }
    }
}
