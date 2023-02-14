using System;
using emt_sdk.Generated.ScenePackage;

namespace emt_sdk.Packages
{
	public interface IPackageRunner
	{
		void RunPackage(PackageDescriptor package);
	}
}

