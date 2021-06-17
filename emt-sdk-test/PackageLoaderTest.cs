using emt_sdk.ScenePackage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace emt_sdk_test
{
    [TestClass]
    public class PackageLoaderTest
    {
        [TestMethod]
        public void TestLoadValid()
        {
            PackageLoader loader = new PackageLoader();
            loader.LoadPackage(File.OpenRead("ScenePackages/package-desc-example.json"));
        }

        [TestMethod]
        public void TestLoadInvalid()
        {
            PackageLoader loader = new PackageLoader();
            Assert.ThrowsException<InvalidDataException>(() =>
            {
                loader.LoadPackage(File.OpenRead("ScenePackages/package-desc-example-invalid.json"));
            });
        }

        [TestMethod]
        public void TestLoadNull()
        {
            PackageLoader loader = new PackageLoader();
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                loader.LoadPackage(null);
            });
        }
    }
}
