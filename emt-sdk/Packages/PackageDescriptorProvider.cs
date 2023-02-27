using emt_sdk.Settings;

namespace emt_sdk.Packages
{
    public class PackageDescriptorProvider : IConfigurationProvider<PackageDescriptor>
    {
        public PackageDescriptor Configuration { get; set; }

        public bool ConfigurationExists => Configuration != null;

        public void SaveConfiguration()
        {
            throw new System.NotImplementedException();
        }
    }
}