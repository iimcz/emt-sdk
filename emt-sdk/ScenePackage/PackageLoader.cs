using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using emt_sdk.Generated.ScenePackage;

namespace emt_sdk.ScenePackage
{
    public class PackageLoader
    {
        private const string SCHEMA_PATH = @"../emt-common/json/package-schema.json";

        private readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(Converter.Settings);
        private readonly JSchema _schema;

        public PackageLoader(string schema = SCHEMA_PATH)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (!File.Exists(schema)) throw new FileNotFoundException();

            using (StreamReader file = File.OpenText(schema))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                _schema = JSchema.Load(reader);
            }
        }

        public Package LoadPackage(TextReader reader, bool validate = true)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            
            using (var jsonTextReader = new JsonTextReader(reader))
            using (var jsonValidatingReader = new JSchemaValidatingReader(jsonTextReader))
            {
                Package package;

                if (validate)
                {
                    IList<string> messages = new List<string>();

                    jsonValidatingReader.Schema = _schema;
                    jsonValidatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);
                    package = _jsonSerializer.Deserialize<Package>(jsonValidatingReader);
                    if (messages.Count != 0) throw new InvalidDataException();
                }
                else
                {
                    package = _jsonSerializer.Deserialize<Package>(jsonTextReader);
                }

                if (package == null) throw new InvalidDataException();
                return package;
            }
        }

        public Package LoadPackage(Stream packageStream, bool validate = true)
        {
            if (packageStream == null) throw new ArgumentNullException(nameof(packageStream));
            if (!packageStream.CanRead) throw new IOException();

            using (var sr = new StreamReader(packageStream)) return LoadPackage(sr, validate);
        }
    }
}
