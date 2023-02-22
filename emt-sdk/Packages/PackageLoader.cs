using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace emt_sdk.Packages
{
    // TODO: Add proper interface
    public class PackageLoader
    {
        private const string SCHEMA_PATH = @"../emt-common/json/package-schema.json";
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(Converter.Settings);
        private readonly JSchema _schema;

        public PackageLoader(string schema = SCHEMA_PATH)
        {
            if (schema == null)
            {
                Logger.Warn("Creating PackageLoader without any schema!");
                return;
            }

            if (!File.Exists(schema)) throw new FileNotFoundException();

            using (StreamReader file = File.OpenText(schema))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                _schema = JSchema.Load(reader);
            }
        }

        public PackageDescriptor LoadPackage(string packageDirectory, bool validate = true)
        {
            if (!Directory.Exists(packageDirectory)) throw new DirectoryNotFoundException();

            var packageFile = Path.Combine(packageDirectory, "package.json");
            if (!File.Exists(packageFile)) throw new FileLoadException();

            using (var reader = File.OpenRead(packageFile)) return LoadPackage(reader, validate);
        }

        public PackageDescriptor LoadPackage(TextReader reader, bool validate = true)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            
            using (var jsonTextReader = new JsonTextReader(reader))
            using (var jsonValidatingReader = new JSchemaValidatingReader(jsonTextReader))
            {
                PackageDescriptor package;

                if (validate)
                {
                    IList<string> messages = new List<string>();

                    jsonValidatingReader.Schema = _schema;
                    jsonValidatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);
                    package = _jsonSerializer.Deserialize<PackageDescriptor>(jsonValidatingReader);
                    if (messages.Count != 0) throw new InvalidDataException();
                }
                else
                {
                    package = _jsonSerializer.Deserialize<PackageDescriptor>(jsonTextReader);
                }

                if (package == null) throw new InvalidDataException();
                return package;
            }
        }

        public PackageDescriptor LoadPackage(Stream packageStream, bool validate = true)
        {
            if (packageStream == null) throw new ArgumentNullException(nameof(packageStream));
            if (!packageStream.CanRead) throw new IOException();

            using (var sr = new StreamReader(packageStream)) return LoadPackage(sr, validate);
        }

        public List<PackageDescriptor> EnumeratePackages(bool validate = true)
        {
            var packages = new List<PackageDescriptor>();
            foreach (var package in Directory.EnumerateDirectories(PackageDescriptor.PackageStore))
            {
                try
                {
                    var desc = LoadPackage(package, validate);
                    desc.Package.Checksum = Path.GetFileName(package);
                    packages.Add(desc);
                }
                catch
                {
                    Logger.Warn($"Malformed or invaid package detected in '{package}', ignoring");
                    // ignored
                }
            }

            return packages;
        }

        /// <summary>
        /// Attempts to find a package with a specified id. Returns null if package is not found.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="validate"></param>
        /// <returns></returns>
        public PackageDescriptor FindPackage(string id, bool validate = true)
        {
            var packages = EnumeratePackages(validate);
            return packages.FirstOrDefault(p => p.Metadata.Id == id);
        }
    }
}
