using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using emt_sdk.Generated.ScenePackages;

namespace emt_sdk.ScenePackages
{
    public class PackageLoader
    {
        private const string SCHEMA_PATH = @"emt-common/json/package-schema.json";

        private readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(Converter.Settings);
        private readonly JSchema _schema;

        public PackageLoader(string schema = SCHEMA_PATH)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (!File.Exists(schema)) throw new FileNotFoundException();

            using (StreamReader file = File.OpenText(SCHEMA_PATH))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                _schema = JSchema.Load(reader);
            }
        }

        public Package LoadPackage(Stream packageStream, bool validate = true)
        {
            if (packageStream == null) throw new ArgumentNullException(nameof(packageStream));
            if (!packageStream.CanRead) throw new IOException();

            using (var sr = new StreamReader(packageStream))
            using (var jsonTextReader = new JsonTextReader(sr))
            using (var jsonValidatingReader = new JSchemaValidatingReader(jsonTextReader))
            {
                IList<string> messages = new List<string>();
                if (validate)
                {
                    jsonValidatingReader.Schema = _schema;
                    jsonValidatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);
                }

                var package = _jsonSerializer.Deserialize<Package>(jsonValidatingReader);
                if (package == null) throw new InvalidDataException();
                if (messages.Count != 0) throw new InvalidDataException();

                return package;
            }
        }
    }
}
